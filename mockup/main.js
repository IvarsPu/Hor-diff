
var TreeExtraClasses = {ServiceType : "service_ok", ServiceChanged: "service_changed", ServiceDeleted: "service_deleted", ServiceNew: "service_new", ServiceError: "service_error",
						DocumentType : "doc_ok", DocumentChanged: "doc_changed", DocumentDeleted: "doc_deleted", DocumentNew: "doc_new", DocumentError: "doc_error" };

var Version1 = "515.13";
var Version2 = "515.21";
var VersionText1 = { data : "", receivedOK : false};
var VersionText2 = { data : "", receivedOK : false};
var isTreeLoaded = false;
var JsonVersion1 = { data : "", receivedOK : false};
var JsonVersion2 = { data : "", receivedOK : false};


$(document).ready(function() {
	PopulateVersionsSelect("#Version1", Version1, JsonVersion1);
	PopulateVersionsSelect("#Version2", Version2, JsonVersion2);
	

	$("#expand_tree").click(function() {
 		 $("#tree").fancytree("getTree").visit(function(node){
        	node.setExpanded();
 		 });
	});

	$("#colapse_tree").click(function() {
 		 $("#tree").fancytree("getTree").visit(function(node){
        	node.setExpanded(false);
 		 });        	
	});	

	$("#tree").fancytree({source:  JsonTree, 
		activate: function(event, data){
			$("#restPath").empty();
			setChangeStatus(data.node);
			$("#diff_frame").attr("src", "");

			if( !IsFile(data.node)) {
				console.log(data.node);
				return false;
			}
			if (data.node.extraClasses == TreeExtraClasses.DocumentError) {
				
				var fragment = document.createDocumentFragment();
								
				data.node.data.errorMessages.forEach(function(element) {
				  
					color = 'red';
					div = document.createElement('div');
					div.style.color = color;
					div.appendChild(document.createTextNode(element));
					fragment.appendChild(div);
					br = document.createElement('br');
					fragment.appendChild(br);					
					display.append(fragment);

				});

			} else {
				// A node was activated:
				if (data.node.data.diffHtmlFile) {
					$("#diff_frame").attr("src", "REST_DIFF/" + data.node.data.diffHtmlFile);
				}
			}

			var restUrl = getNodeRestUrl(data.node);
			$("#restPath").append(restUrl);
		}});
		
 
	 
	  $(window).resize(function(){
		setDivSize();	
	  });


	function setDivSize() 
	{
		var divTop = $('#change_panel').position().top  + $('#diff_frame').position().top; 
		var newHeight = $(window).height() - divTop - 30;
		
		$('#diff_frame').height(newHeight);
		$('#tree').height(newHeight - 35);	
	}
	
	setDivSize(); 
});

function PopulateVersionsSelect(id, versionInfo, JsonVersion){
	$(id).empty();
	$(id).append("<option value=" + versionInfo + ">" + versionInfo + "</option>");

}

function DocumentReceived(){

	if(JsonVersion1.receivedOK && JsonVersion2.receivedOK ){
		diffJson = JSON.parse(JSON.stringify(JsonVersion1.data));
		MarkSchemaDifferences(diffJson, JsonVersion2.data);

		var radioResult = $('input[name=optradio]:checked').val() - 0;
		if(radioResult === 2) {		
			filterModifiedServicesOnly(diffJson);
		} 

			//$('#tree').fancytree('option', 'source', diffJson);
			$("#tree").fancytree('getTree').reload(diffJson);
	}
}

function setChangeStatus(treeNode) 
{
	var status = "";

	if (treeNode) {

		switch(treeNode.extraClasses) {

			case TreeExtraClasses.ServiceType:			
			case TreeExtraClasses.DocumentType:
				status = "Bez izmaiņām";
				break;
			case TreeExtraClasses.ServiceChanged:			
			case TreeExtraClasses.DocumentChanged:
				status = "Modificēts";
				break;
			case TreeExtraClasses.ServiceDeleted:			
			case TreeExtraClasses.DocumentDeleted:
				status = "Dzēsts";
				break;
			case TreeExtraClasses.ServiceNew:			
			case TreeExtraClasses.DocumentNew:
				status = "Jauns";
				break;	
			case TreeExtraClasses.DocumentError:			
				status = "Kļūdains";
				break;					
		}											
	}

	$("#changeStatus").empty();
	$("#changeStatus").append(status);
}

function getNodeRestUrl(treeNode) 
{
	return "/rest/" + treeNode.parent.title + "/" +treeNode.title;
}

function MarkSchemaDifferences(jsonVer1Array, jsonVer2Array) 
{
	var rootContainer = { isError : false };
	
	for (var i = 0; i < jsonVer1Array.length; i++) {
		MarkServiceDifferences(jsonVer1Array[i], jsonVer2Array, "", rootContainer);
	}
	CheckForNewTreeItems(jsonVer1Array, jsonVer2Array); 	
}

function MarkServiceDifferences(ver1Service, jsonVer2Array,  parentRestPath, errStatusContainer) 
{
	var isDifferent = false;
	ver1Service.restPath = "";
	ver1Service.isError = false;

	if(parentRestPath != "") {
		ver1Service.restPath = parentRestPath + "/";
	}
	ver1Service.restPath += ver1Service.title;	

	if (ver1Service.children && ver1Service.title) {
		var ver2Service = jsonVer2Array.find(item => item.title === ver1Service.title);

		if (ver2Service && ver2Service.children) {

			// Check modified and deleted
			for (var f = 0; f < ver1Service.children.length; f++){
				var ver1ServiceChild = ver1Service.children[f];

				if(ver1ServiceChild.extraClasses === TreeExtraClasses.DocumentType) {
					isDifferent = MarkDocumentDifferences(ver1ServiceChild, ver2Service.children, ver1Service.restPath, ver1Service.title, ver1Service) || isDifferent;
				} else {
					isDifferent = MarkServiceDifferences(ver1ServiceChild, ver2Service.children, ver1Service.restPath, ver1Service) || isDifferent;
				}
			}

			// Check the new ones
			isDifferent = CheckForNewTreeItems(ver1Service.children, ver2Service.children) || isDifferent; 			
			
			if (isDifferent) {
				ver1Service.extraClasses = TreeExtraClasses.ServiceChanged;
			}

		} else {
			ver1Service.extraClasses = TreeExtraClasses.ServiceDeleted;
		}
	}
	errStatusContainer.isError = ver1Service.isError || errStatusContainer.isError;
	return isDifferent;
}

function CheckForNewTreeItems(jsonVer1Array, jsonVer2Array) 
{
	var isDifferent = false;
		
	for (var f = 0; f < jsonVer2Array.length; f++){
		var ver2ServiceChild = jsonVer2Array[f];
		var ver1ServiceChild = jsonVer1Array.find(item => item.title === ver2ServiceChild.title);

		if(!ver1ServiceChild) {
			markTreeAsNew(ver2ServiceChild);
			jsonVer1Array.push(ver2ServiceChild);
			isDifferent = true;
		}
	}
	return isDifferent;
}

function MarkDocumentDifferences(jsonVer1Doc, jsonVer2DocArray, parentRestPath, parentWebPath, statusContainer) 
{
	jsonVer1Doc.restPath = parentRestPath;
	jsonVer1Doc.parentName = parentWebPath;	

	var jsonVer2Doc = jsonVer2DocArray.find(item => item.title === jsonVer1Doc.title);
	
	if(jsonVer1Doc.errorMessage || (jsonVer2Doc && jsonVer2Doc.errorMessage)) {
		jsonVer1Doc.isError = true;
		statusContainer.isError = true;
		jsonVer1Doc.extraClasses = TreeExtraClasses.DocumentError;
		isDifferent = getErrorDiff(jsonVer1Doc, jsonVer2Doc);
	} else {

		if (jsonVer2Doc) {
			var isDifferent;
			if ($("#ignore_namespaces").is(':checked')) {
				isDifferent = (jsonVer1Doc.noNamspaceHashCode != jsonVer2Doc.noNamspaceHashCode);
			} else {
				isDifferent = (jsonVer1Doc.hashCode != jsonVer2Doc.hashCode);
			}

			if (isDifferent) {
				jsonVer1Doc.extraClasses =  TreeExtraClasses.DocumentChanged;
			}		
		} else {
			isDifferent = true;
			jsonVer1Doc.extraClasses =  TreeExtraClasses.DocumentDeleted;
		}
	}		
	
	return isDifferent;
}

function getErrorDiff(jsonVer1Doc, jsonVer2Doc) {
	var msg1 = "", msg2 = "";
	jsonVer1Doc.errorMessages = [];
	

	if(jsonVer1Doc.errorMessage) {
		msg1 = "1. Versijas kļūda: HttpCode: " + jsonVer1Doc.httpCode + ", " + jsonVer1Doc.errorMessage;
		jsonVer1Doc.errorMessages.push(msg1);
	}

	if(jsonVer2Doc && jsonVer2Doc.errorMessage) {
		msg2 = "2. Versijas kļūda: HttpCode: " + jsonVer2Doc.httpCode + ", " + jsonVer2Doc.errorMessage;
		jsonVer1Doc.errorMessages.push(msg2);
	}
	return msg1 != msg2;
}

function markTreeAsNew(root) {
	if(root.extraClasses === TreeExtraClasses.DocumentType) {
		root.extraClasses = TreeExtraClasses.doc_new;
	} else {
		root.extraClasses = TreeExtraClasses.service_new;

		if (root.children) {
			
			for (var f = 0; f < root.children.length; f++) {
				markTreeAsNew(root.children[f]);
			}
		}
	}
}

function filterModifiedServicesOnly(jsonVer1Array) {

	for (var i = jsonVer1Array.length -1; i >= 0; i--) {
		
		if(jsonVer1Array[i].extraClasses === TreeExtraClasses.ServiceType) {
			jsonVer1Array.splice(i, 1);
		} else if (jsonVer1Array[i].children) {
			filterModifiedServicesOnly(jsonVer1Array[i].children);
		} 
	}
}


function IsFile(treeNode){
	var isFile = false;
						
	if (treeNode){
		if (treeNode.extraClasses == TreeExtraClasses.DocumentType 
				|| treeNode.extraClasses == TreeExtraClasses.DocumentChanged
				|| treeNode.extraClasses == TreeExtraClasses.DocumentDeleted				
				|| treeNode.extraClasses == TreeExtraClasses.DocumentNew
				|| treeNode.extraClasses == TreeExtraClasses.DocumentError) {
			isFile = true;
		}
	}
	return isFile;

}


