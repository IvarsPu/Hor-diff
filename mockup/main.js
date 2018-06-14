if (window.File && window.FileReader && window.FileList && window.Blob) {
    // Great success! All the File APIs are supported.
} else {
    alert('The File APIs are not fully supported in this browser.');
}

var VersionText1, VersionText2;
window.onload = function () {
var versionButton1 = document.getElementById('versionButton1');
var versionButton2 = document.getElementById('versionButton2');

versionButton1.addEventListener('change', function(e) {
    //Get the file object 
    var fileTobeRead = versionButton1.files[0];

    //Initialize the FileReader object to read the 2file 
    var fileReader = new FileReader();
    fileReader.onload = function(e) {
        VersionText1 = fileReader.result
        //console.log(VersionText1);
    }
    fileReader.readAsText(fileTobeRead);
	if (VersionText2 != null) {
		
	}

}, false);

versionButton2.addEventListener('change', function(e) {
    //Get the file object 
    var fileTobeRead = versionButton2.files[0];

    //Initialize the FileReader object to read the 2file 
    var fileReader = new FileReader();
    fileReader.onload = function(e) {
        VersionText2 = fileReader.result
        //console.log(VersionText2);
		if (VersionText1 != null && VersionText2 != null) {
			DoDiff(VersionText1, VersionText2, 3);
		}
    }
    fileReader.readAsText(fileTobeRead);

}, false);
}

function escapeRegExp(str) {
    return str.replace(/([.*+?^=!:${}()|\[\]\/\\])/g, "\\$1");
}

function replaceAll(str, find, replace) {
    return str.replace(new RegExp(escapeRegExp(find), 'g'), replace);
}

function loadDoc(path) {
    var xhttp = new XMLHttpRequest();
    xhttp.open("GET", path, true);
    xhttp.send();
    console.log(xhttp.responseText);
	return xhttp.responseText;
}


function DoDiff(first, second, type){
var color = '',
    span = null,
	diff;
	console.log("Diff called");
	

switch(type){
	case 1: 
	diff = JsDiff.diffChars(VersionText1, VersionText2);
	break;
	case 2:
	diff = JsDiff.diffWords(VersionText1, VersionText2);
	break;
	case 3:
	diff = JsDiff.diffLines(VersionText1, VersionText2, false, true);
	break;
}
	
//var diff = JsDiff.diffLines(first, second);
var diff = JsDiff.diffLines(VersionText1, VersionText2);
/*
var myDiff = new JsDiff.Diff();
myDiff.tokenize = function(value) {
  return value.split(/(\n|\r\n)/);
};
var diff = myDiff.diff(first, second);
var diff = JsDiff.diffLines(VersionText1, VersionText2);
.replace(String.fromCharCode(13, 10),"\n");
.replace(">","><br>");
*/
var  display = document.getElementById('change_content'),
    fragment = document.createDocumentFragment();
	
	

for (var i = 0; i < diff.length; i++){
  // green for additions, red for deletions
  // grey for common parts
  color = diff[i].added ? 'green' :
    diff[i].removed ? 'red' : 'grey';
  span = document.createElement('span');
  span.style.color = color;
  span.appendChild(document
    .createTextNode(diff[i].value));
  fragment.appendChild(span);
}

display.appendChild(fragment);
}

