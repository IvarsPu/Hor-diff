 var versions = [
	{version: "510.8", url:"510.8"},
	{version: "515.3", url:"515.3"}];

var TreeExtraClasses = {ServiceChanged: "service_changed", DocumentChanged: "doc_changed", DocumentType : "doc_ok", ServiceType : "service_ok"};

var VersionText1 = { data : "", receivedOK : false};
var VersionText2 = { data : "", receivedOK : false};
var isTreeLoaded = false;
var JsonVersion1 = { data : "", receivedOK : false};
var JsonVersion2 = { data : "", receivedOK : false};
diffJson = {};


$(document).ready(function() {
	$("#Version1").change(function() {
		var url = "rest/" + $("#Version1 option:selected").val() + "/version.json";
		getFileAjax( url, "json", JsonVersion1);
	});
	$("#Version2").change(function() {
		var url = "rest/" + $("#Version2 option:selected").val() + "/version.json";
		getFileAjax( url, "json", JsonVersion2);
	});
	PopulateVersionsSelect("#Version1", versions);
	PopulateVersionsSelect("#Version2", versions);
	$('input[name=optradio]').change(function() {
		DiffRecieved(Version1, Version2);
	});
});

function PopulateVersionsSelect(id, versionInfo){
	$(id).empty();
	$(id).append("<option>--Select--</option>");
	$(versionInfo).each(function(i) { 
		$(id).append("<option value=" + versionInfo[i].url + ">" + versionInfo[i].version + "</option>");
	});
}



function DocumentReceived(){
	if(JsonVersion1.receivedOK && JsonVersion2.receivedOK ){
	diffJson = JSON.parse(JSON.stringify(JsonVersion1.data));
	MarkSchemaDifferences(diffJson, JsonVersion2.data);
	if(!isTreeLoaded){
		$("#tree").fancytree({source: diffJson, 
		activate: function(event, data){
			if( data.node.extraClasses === TreeExtraClasses.ServiceChanged || data.node.extraClasses === TreeExtraClasses.ServiceType){
				console.log(data.node);
				return false;
			}
			// A node was activated:
			var path1 = "rest/" + $("#Version1 option:selected").val() + data.node.data.parentName + "/" + data.node.title;
			var path2 = "rest/" + $("#Version2 option:selected").val() + data.node.data.parentName + "/" + data.node.title;
			getFileAjax(path1, "text", VersionText1);
			getFileAjax(path2, "text", VersionText2);
			$("#restPath").empty();
			$("#restPath").append("/rest" + data.node.data.parentName + "/" + data.node.title);
		}});
	}
	else
	{
		$("#tree").fancytree('getTree').reload(diffJson);
	}
	}
}

function MarkSchemaDifferences(jsonVer1, jsonVer2) {
	for (var i = 0; i < jsonVer1.length; i++){
		MarkServiceDifferences(jsonVer1[i],jsonVer2[i], "");
	}
}

function MarkServiceDifferences(jsonVer1, jsonVer2, parentName) {
	jsonVer1.parentName = parentName;
	var isDifferent = false;
	if (jsonVer1.children){
		
	for(var f = 0; f < jsonVer1.children.length; f++){
		if(jsonVer1.children[f].extraClasses === TreeExtraClasses.DocumentType) {
			isDifferent = MarkDocumentDifferences(jsonVer1.children[f], jsonVer2.children[f], jsonVer1.parentName + "/" + jsonVer1.title) || isDifferent;
		}
		else
		{
			isDifferent = MarkServiceDifferences(jsonVer1.children[f], jsonVer2.children[f], jsonVer1.parentName + "/" + jsonVer1.title) || isDifferent;
		}
	}
	if (isDifferent)
		jsonVer1.extraClasses = TreeExtraClasses.ServiceChanged;
	}
	return isDifferent;
}

function MarkDocumentDifferences(jsonVer1, jsonVer2, parentName) {
	jsonVer1.parentName = parentName;
	var isDifferent = (jsonVer1.hashCode !== jsonVer2.hashCode);
	
	if (isDifferent) {
		jsonVer1.extraClasses =  TreeExtraClasses.DocumentChanged;
	}
	return isDifferent;
}

function DiffRecieved(){
	if (VersionText1.data !== "" && VersionText2.data !== ""){
		DoDiff(VersionText1.data, VersionText2.data);
	}
}


function DoDiff(first, second) {
    var color = '',
        span = null,
        diff = null;
    console.log("Diff called");
	var radioResult = $('input[name=optradio]:checked').val() - 0;
//console.log(radioResult);
if(radioResult === 1){
            diff = JsDiff.diffChars(first, second);
}
else if(radioResult === 2){
    diff = JsDiff.diffWords(first, second);
}
else if(radioResult === 3){
	console.log(JsDiff.diffLines(first, second, false, true));
    diff = JsDiff.diffLines(first, second, false, true);
	
} 
	//console.log(diff);
    var display = $("#change_content");
	display.children().remove()
    fragment = document.createDocumentFragment();
	


    for (var i = 0; i < diff.length; i++) {
        // green for additions, red for deletions
        // grey for common parts
        color = diff[i].added ? '#29e514' :
            diff[i].removed ? 'red' : 'grey';
        span = document.createElement('span');
        span.style.color = color;
        span.appendChild(document
            .createTextNode(diff[i].value));
        fragment.appendChild(span);
    }
	
    display.append(fragment);
	$("#changeStatus").empty();
	
	if(display.children().length > 1){
		$("#changeStatus").append("Modificēts");
	}
	else {
		$("#changeStatus").append("Nemodificēts");
	}
}


function getFileAjax(path, datatype, ResultObject){
        /**
         * http://api.jquery.com/jQuery.ajax/
         */
        $.ajax({
            url: path,

            type: "GET",

            dataType: datatype,

            /**
             * A function to be called if the request fails. 
             */
            error: function(jqXHR, textStatus, errorThrown) {
                //alert('An error occurred... Look at the console (F12 or Ctrl+Shift+I, Console tab) for more information!');

                //$('#result').html('<p>status code: '+jqXHR.status+'</p><p>errorThrown: ' + errorThrown + '</p><p>jqXHR.responseText:</p><div>'+jqXHR.responseText + '</div>');
                console.log('jqXHR:');
                console.log(jqXHR);
                console.log('textStatus:');
                console.log(textStatus);
                console.log('errorThrown:');
                console.log(errorThrown);
            },

            /**
             * A function to be called if the request succeeds.
             */
            success: function(data, textStatus, jqXHR) {
                //$('#result').html(data);
                //alert('Load was performed. Look at the console (F12 or Ctrl+Shift+I, Console tab) for more information! ');
				ResultObject.data = data;
				ResultObject.receivedOK = true;
                //console.log('jqXHR:');
                //console.log(jqXHR);
                //console.log('textStatus:');
                //console.log(textStatus);
                //console.log('data:');
                //console.log(data);
				if (datatype == "json"){
					DocumentReceived();
				}
				else if (datatype == "text"){
					DiffRecieved();
				}
            }
        });

    }
