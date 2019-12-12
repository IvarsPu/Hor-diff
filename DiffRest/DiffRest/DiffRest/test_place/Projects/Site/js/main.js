﻿
var TreeExtraClasses = {ServiceType : "service_ok", ServiceChanged: "service_changed", ServiceDeleted: "service_deleted", ServiceNew: "service_new", ServiceError: "service_error",
						DocumentType : "doc_ok", DocumentChanged: "doc_changed", DocumentDeleted: "doc_deleted", DocumentNew: "doc_new", DocumentError: "doc_error" };



var selectedId = 0;


$(document).ready(function() {
	$("#Version1").append("<option value=" + firstVersion + ">" + firstVersion + "</option>");
	$("#Version2").append("<option value=" + secondVersion + ">" + secondVersion + "</option>");
	

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

	$("#tree").fancytree({extensions: ["filter"],source:  JsonTree,
			filter: {
				autoExpand: true,
				mode: "hide"
			},
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
					selectedId = 0;
				}
			}

			var restUrl = data.node.data.diffHtmlFile;
			$("#restPath").append(restUrl);
		}});
		
	$("input[name=search]").keyup(function(){
		var n = $("#tree").fancytree("getTree").filterBranches($(this).val());
	}).focus();
	
	$("#next").click(function() {
 		var iframe = document.getElementById("diff_frame");
		var elmnts = iframe.contentWindow.document.getElementsByTagName("span");
		
		if(selectedId < elmnts.length - 2){	
			var pass = true;
			while(pass){
				selectedId = selectedId + 1;
				
				if(selectedId == elmnts.length - 2 || (elmnts[selectedId - 1].className == "" && elmnts[selectedId].className != "")){
					pass = false;
				}
			}
		}
		
		elmnts[selectedId].scrollIntoView( true );
	});
	
	$("#previous").click(function() {
 		var iframe = document.getElementById("diff_frame");
		var elmnts = iframe.contentWindow.document.getElementsByTagName("span");
		
		if(selectedId > 1){	
			var pass = true;
			while(pass){
				selectedId = selectedId - 1;
				
				if(selectedId == 0 || (elmnts[selectedId - 1].className == "" && elmnts[selectedId].className != "")){
					pass = false;
				}
			}
		}
		
		elmnts[selectedId].scrollIntoView( true );
	});

	setDivSize();

    $(window).resize(function () {
        setDivSize();
    });
});

function setDivSize() {
    var newHeight = $(window).height() - 200;

    $('#treeControl').height(newHeight);
    $('#htmlControl').height(newHeight);
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