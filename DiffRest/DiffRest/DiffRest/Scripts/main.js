var TreeExtraClasses = {
    ServiceType: "service_ok", ServiceChanged: "service_changed", ServiceDeleted: "service_deleted", ServiceNew: "service_new", ServiceError: "service_error",
    DocumentType: "doc_ok", DocumentChanged: "doc_changed", DocumentDeleted: "doc_deleted", DocumentNew: "doc_new", DocumentError: "doc_error"
};

var JsonVersion1 = { data: "", receivedOK: false };
var JsonVersion2 = { data: "", receivedOK: false };
var selectedId = 0;

var versions;
var JsonTree = [];

$(document).ready(function () {
    loadVersionsAjax();

    $("#Version1").change(function () {
        PopulateReleaseSelect("#Release1", $("#Version1 option:selected").val());
    });

    $("#Release1").change(function () {
        var url = "/Home/GetFile?filePath=" + $("#Version1 option:selected").val() + "/" + $("#Release1 option:selected").val() + "/metadata.xml";
        getFileAjax(url, "xml", JsonVersion1);		
    });

    $("#Version2").change(function () {
        PopulateReleaseSelect("#Release2", $("#Version2 option:selected").val());
    });

    $("#Release2").change(function () {
        var url = "/Home/GetFile?filePath=" + $("#Version2 option:selected").val() + "/" + $("#Release2 option:selected").val() + "/metadata.xml";
        getFileAjax(url, "xml", JsonVersion2);			
    });

    $('#download').click(function () {
        var first = $("#Version1 option:selected").val() + "/" + $("#Release1 option:selected").val();
        var second = $("#Version2 option:selected").val() + "/" + $("#Release2 option:selected").val();
        if ($("#Version1 option:selected").val() != "--Select--" && $("#Version2 option:selected").val() != "--Select--"
            && first != second) {
            window.location = "/Home/LoadFile?first=" + first + "&second=" + second;
        }
    });

    $('input[name=optradio]').change(function () {
        DocumentReceived();
    });

    $('input[name=ignore_namespaces]').change(function () {
        DocumentReceived();
    });
    
    $("#expand_tree").click(function () {
        $("#tree").fancytree("getTree").visit(function (node) {
            node.setExpanded();
        });
    });

    $("#colapse_tree").click(function () {
        $("#tree").fancytree("getTree").visit(function (node) {
            node.setExpanded(false);
        });
    });

    $("#tree").fancytree({
        extensions: ["filter"], source: JsonTree,
        filter: {
            autoExpand: true,
            mode: "hide"
        },
        activate: function (event, data) {
            $("#restPath").empty();
            setChangeStatus(data.node);

            if (!IsFile(data.node)) {
                console.log(data.node);
                return false;
            }
            var path = data.node.data.restPath + "/" + data.node.title;
            if (data.node.extraClasses == TreeExtraClasses.DocumentError) {
                var fragment = document.createDocumentFragment();

                data.node.data.errorMessages.forEach(function (element) {

                    color = 'red';
                    div = document.createElement('div');
                    div.style.color = color;
                    div.appendChild(document.createTextNode(element));
                    fragment.appendChild(div);
                    br = document.createElement('br');
                    fragment.appendChild(br);
                });

            } else {
                var path1 = $("#Version1 option:selected").val() + "/" + data.node.data.storedRelease + "/" + path;
                var path2 = $("#Version2 option:selected").val() + "/" + data.node.data.storedRelease2 + "/" + path;
                GetChanges(path1, path2);
                selectedId = 0;
            }
            
            $("#restPath").append(path);
        }
    });

    $("input[name=search]").keyup(function () {
        $("#tree").fancytree("getTree").filterBranches($(this).val());
    }).focus();

    $("#next").click(function () {
        var iframe = document.getElementById("diff_frame");
        var elmnts = iframe.contentWindow.document.getElementsByTagName("span");

        if (selectedId < elmnts.length - 2) {
            var pass = true;
            while (pass) {
                selectedId = selectedId + 1;

                if (selectedId == elmnts.length - 2 || (elmnts[selectedId - 1].className == "" && elmnts[selectedId].className != "")) {
                    pass = false;
                }
            }
        }

        elmnts[selectedId].scrollIntoView(true);
    });

    $("#previous").click(function () {
        var iframe = document.getElementById("diff_frame");
        var elmnts = iframe.contentWindow.document.getElementsByTagName("span");

        if (selectedId > 1) {
            var pass = true;
            while (pass) {
                selectedId = selectedId - 1;

                if (selectedId == 0 || (elmnts[selectedId - 1].className == "" && elmnts[selectedId].className != "")) {
                    pass = false;
                }
            }
        }

        elmnts[selectedId].scrollIntoView(true);
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

function showPage() {
    $('#VersionControl :input').attr('disabled', false);
    $(":ui-fancytree").fancytree("enable");
    document.getElementById("loader").style.display = "none";
}

function showLoad() {
    $('#VersionControl :input').attr('disabled', true);
    $(":ui-fancytree").fancytree("disable");
    document.getElementById("loader").style.display = "block";
}

function loadVersionsAjax() {
    showLoad();
    $.ajax({
        url: '/Home/GetVersions/',
        dataType: 'xml',
        error: function () {
            showPage();
        },
        success: function (data) {
            versions = getVersionList(data);

            PopulateVersionsSelect("#Version1");
            PopulateVersionsSelect("#Version2");
            showPage();
        }
    });
}

function getVersionList(xmlVersions) {
    var versionArray = [];

    var xml = $(xmlVersions);

    xml.find("Version").each(function () {
        var name = this.textContent;
        var releaseArray = [];
        $(this.parentNode).find("Release").each(function () {
            releaseArray.push(this.textContent);
        });

        let version = {
            name: name,
            releases: releaseArray
        };
        versionArray.push(version);
    });

    return versionArray;
}

function PopulateVersionsSelect(version) {
    $(version).empty();
    $(version).append("<option>--Select--</option>");
    $(versions).each(function (i, data) {
        $(version).append("<option value=" + data.name + ">" + data.name + "</option>");
    });
}

function PopulateReleaseSelect(release, selected) {
    $(release).empty();

    if (selected != "--Select--") {
        var found = versions.filter(function (item) { return item.name === selected; });
        found[0].releases.forEach(function (text) {
            $(release).append("<option value=" + text + ">" + text + "</option>");
        });
    }

    $(release).change();
}

function GetChanges(file1, file2) {
    showLoad();
    $.ajax({
        url: "/Home/DiffColor?firstFile=" + file1 + "&secondFile=" + file2,
        error: function () {
            showPage();
        },
        success: function (data) {
            $("#diff_frame").contents().find('html').html(data);
            showPage();
        }
    });
}







function DocumentReceived() {

    if (JsonVersion1.receivedOK && JsonVersion2.receivedOK) {
        JsonTree = JSON.parse(JSON.stringify(JsonVersion1.data));
        MarkSchemaDifferences(JsonTree, JsonVersion2.data);

        var radioResult = $('input[name=optradio]:checked').val() - 0;
        if (radioResult === 2) {
            filterModifiedServicesOnly(JsonTree);
        }
        
        $("#tree").fancytree('getTree').reload(JsonTree);
    }
}

function setChangeStatus(treeNode) {
    var status = "";

    if (treeNode) {

        switch (treeNode.extraClasses) {

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

function MarkSchemaDifferences(jsonVer1Array, jsonVer2Array) {
    var rootContainer = { isError: false };

    for (var i = 0; i < jsonVer1Array.length; i++) {
        MarkServiceDifferences(jsonVer1Array[i], jsonVer2Array, "", rootContainer);
    }
    CheckForNewTreeItems(jsonVer1Array, jsonVer2Array, "");
}

function FindItemByTitle(JsonArray, title) {
    for (var i = 0; i < JsonArray.length; ++i) {
        if (JsonArray[i].title == title) {
            return JsonArray[i];
        }
    }
}

function MarkServiceDifferences(ver1Service, jsonVer2Array, parentRestPath, errStatusContainer) {
    var isDifferent = false;
    ver1Service.restPath = "";
    ver1Service.isError = false;
    
    ver1Service.restPath += parentRestPath;

    if (ver1Service.children && ver1Service.title) {
        var ver2Service = FindItemByTitle(jsonVer2Array, ver1Service.title);


        if (ver2Service && ver2Service.children) {

            // Check modified and deleted
            for (var f = 0; f < ver1Service.children.length; f++) {
                var ver1ServiceChild = ver1Service.children[f];

                if (ver1ServiceChild.extraClasses === TreeExtraClasses.DocumentType) {
                    isDifferent = MarkDocumentDifferences(ver1ServiceChild, ver2Service.children, ver1Service.restPath + "/" + ver1Service.title, ver1Service.title, ver1Service) || isDifferent;
                } else {
                    isDifferent = MarkServiceDifferences(ver1ServiceChild, ver2Service.children, ver1Service.restPath + "/" + ver1Service.title, ver1Service) || isDifferent;
                }
            }

            // Check the new ones
            isDifferent = CheckForNewTreeItems(ver1Service.children, ver2Service.children, ver1Service.restPath + "/" + ver1Service.title) || isDifferent;

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

function CheckForNewTreeItems(jsonVer1Array, jsonVer2Array, parantPath) {
    for (var f = 0; f < jsonVer2Array.length; f++) {
        var ver2ServiceChild = jsonVer2Array[f];

        var ver1ServiceChild = FindItemByTitle(jsonVer1Array, ver2ServiceChild.title);

        if (!ver1ServiceChild) {
            markTreeAsNew(ver2ServiceChild, parantPath);
            jsonVer1Array.push(ver2ServiceChild);
        }
    }
}

function MarkDocumentDifferences(jsonVer1Doc, jsonVer2DocArray, parentRestPath, parentWebPath, statusContainer) {
    jsonVer1Doc.restPath = parentRestPath;
    jsonVer1Doc.parentName = parentWebPath;
    
    var jsonVer2Doc = FindItemByTitle(jsonVer2DocArray, jsonVer1Doc.title);

    if (jsonVer1Doc.errorMessage || (jsonVer2Doc && jsonVer2Doc.errorMessage)) {
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
                jsonVer1Doc.extraClasses = TreeExtraClasses.DocumentChanged;
            }
            jsonVer1Doc.storedRelease2 = jsonVer2Doc.storedRelease;
        } else {
            isDifferent = true;
            jsonVer1Doc.extraClasses = TreeExtraClasses.DocumentDeleted;
            jsonVer1Doc.storedRelease2 = -1;            
        }
    }

    return isDifferent;
}

function getErrorDiff(jsonVer1Doc, jsonVer2Doc) {
    var msg1 = "", msg2 = "";
    jsonVer1Doc.errorMessages = [];


    if (jsonVer1Doc.errorMessage) {
        msg1 = "1. Versijas kļūda: HttpCode: " + jsonVer1Doc.httpCode + ", " + jsonVer1Doc.errorMessage;
        jsonVer1Doc.errorMessages.push(msg1);
    }

    if (jsonVer2Doc && jsonVer2Doc.errorMessage) {
        msg2 = "2. Versijas kļūda: HttpCode: " + jsonVer2Doc.httpCode + ", " + jsonVer2Doc.errorMessage;
        jsonVer1Doc.errorMessages.push(msg2);
    }
    return !jsonVer1Doc.hasOwnProperty('errorMessage') && jsonVer2Doc.hasOwnProperty('errorMessage');
}

//Check here for new
function markTreeAsNew(root, parantPath) {
    root.restPath = parantPath;
    if (root.extraClasses === TreeExtraClasses.DocumentType) {
        root.extraClasses = TreeExtraClasses.DocumentNew;
    } else {
        root.extraClasses = TreeExtraClasses.ServiceNew;

        if (root.children) {
            for (var f = 0; f < root.children.length; f++) {
                markTreeAsNew(root.children[f], root.restPath + "/" + root.title);
            }
        }
    }
}

function filterModifiedServicesOnly(jsonVer1Array) {

    for (var i = jsonVer1Array.length - 1; i >= 0; i--) {

        if (jsonVer1Array[i].extraClasses === TreeExtraClasses.ServiceType) {
            jsonVer1Array.splice(i, 1);
        } else if (jsonVer1Array[i].children) {
            filterModifiedServicesOnly(jsonVer1Array[i].children);
        }
    }
}

function DiffRecieved() {
    if (VersionText1.data !== "" && VersionText2.data !== "") {
        DoDiff(VersionText1.data, VersionText2.data);
    }
}

function IsFile(treeNode) {
    var isFile = false;

    if (treeNode) {
            isFile = true;
    }
    return isFile;

}

function DoDiff(first, second) {
    var color = '',
        span = null,
        diff = null;
    console.log("Diff called");

    //diff = JsDiff.diffWords(first, second);
    diff = JsDiff.diffLines(first, second, false, true);

    //console.log(diff);
    var display = $("#change_content");
    display.children().remove();
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
}

function getFileAjax(path, datatype, ResultObject) {
    $.ajax({
        url: path,

        type: "GET",

        /**
         * A function to be called if the request fails. 
         */
        error: function (jqXHR, textStatus, errorThrown) {
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
        success: function (data, textStatus, jqXHR) {
            var parser = new DOMParser();
            data = parser.parseFromString(data, "text/xml");

            ResultObject.receivedOK = true;
            if (datatype == "json") {
                ResultObject.data = data;
                DocumentReceived();
            } else if (datatype == "text") {
                ResultObject.data = data;
                DiffRecieved();
            } else if (datatype == "xml") {
                ResultObject.data = getTreeJsonFromXmlMetadata(data);
                DocumentReceived();
            }
        }
    });
}

function getTreeJsonFromXmlMetadata(xmlMetadata) {
    var json = [];
 
    var $xml = $(xmlMetadata);

    var rest_api_metadata = $xml.find("rest_api_metadata");

    if (rest_api_metadata) {

        rest_api_metadata.children().each(function () {
            addJsonServiceMetadata($(this), json);
        });
    }
    
    return json;
}

function addJsonServiceMetadata(xmlNode, jsonNode) {

    var serviceNode = {};
    serviceNode.title = xmlNode.attr('name');
    serviceNode.extraClasses = "service_ok";
    serviceNode.type = xmlNode.get(0).tagName;

    if (xmlNode.attr('description')) {
        serviceNode.description = xmlNode.attr('description');
    }

    if (xmlNode.children().length > 0) {
        serviceNode.children = [];

        xmlNode.children().each(function () {
            var tagName = $(this).get(0).tagName;

            if (tagName == "service_group" || tagName == "service" || tagName == "resource") {
                addJsonServiceMetadata($(this), serviceNode.children);
            } else {
                addJsonFileMetadata($(this), serviceNode.children);
            }
        });
    } else {
        serviceNode.children = null;
    }
    jsonNode.push(serviceNode);
}

function addJsonFileMetadata(xmlNode, jsonServiceFiles) {

    var fileNode = {};
    fileNode.title = xmlNode.attr('name');
    fileNode.extraClasses = "doc_ok";
    fileNode.hashCode = xmlNode.attr('hashCode');
    fileNode.noNamspaceHashCode = xmlNode.attr('noNamspaceHashCode');
    fileNode.type = xmlNode.get(0).tagName;
    fileNode.storedRelease = xmlNode.attr('stored_release');

    if (xmlNode.attr('description')) {
        fileNode.description = xmlNode.attr('description');
    }
    if (xmlNode.attr('http_code')) {
        fileNode.httpCode = xmlNode.attr('http_code');
    }
    if (xmlNode.attr('error_message')) {
        fileNode.errorMessage = xmlNode.attr('error_message');
    }

    jsonServiceFiles.push(fileNode);
}