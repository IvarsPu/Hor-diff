var TreeExtraClasses = {
    ServiceType: "service_ok", ServiceChanged: "service_changed", ServiceDeleted: "service_deleted", ServiceNew: "service_new", ServiceError: "service_error",
    DocumentType: "doc_ok", DocumentChanged: "doc_changed", DocumentDeleted: "doc_deleted", DocumentNew: "doc_new", DocumentError: "doc_error"
};

var selectedId = 0;

var JsonTree = [];

$(document).ready(function () {
    loadVersionsAjax();

    $("#Version1").change(function () {
        GetChanges();
    });

    $("#Version2").change(function () {
        GetChanges();
    });

    $('#download').click(function () {
        if ($("#Version1 option:selected").val() != "--Select--" && $("#Version2 option:selected").val() != "--Select--"
                && $("#Version2 option:selected").val() != $("#Version1 option:selected").val()) {
            window.location = "http://localhost:51458/Home/LoadFile?first=" + $("#Version1 option:selected").val() + "&second=" + $("#Version2 option:selected").val();
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
                GetDiffHtml(data.node.data.diffHtmlFile);
                selectedId = 0;
            }

            var restUrl = data.node.data.diffHtmlFile;
            $("#restPath").append(restUrl);
        }
    });

    $("input[name=search]").keyup(function () {
        var n = $("#tree").fancytree("getTree").applyFilter($(this).val());
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
    var divTop = $('#change_panel').position().top + $('#diff_frame').position().top;
    var newHeight = $(window).height() - divTop - 30;

    $('#diff_frame').height(newHeight);
    $('#tree').height(newHeight - 42);
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
        url: 'http://localhost:51458/Home/GetVersions',
        dataType: 'xml',
        error: function () {
            showPage();
        },
        success: function (data) {
            var versions = getVersionList(data);

            PopulateVersionsSelect("#Version1", versions);
            PopulateVersionsSelect("#Version2", versions);
            showPage();
        }
    });
}

function getVersionList(xmlVersions) {
    var versionArray = [];

    var xml = $(xmlVersions);

    xml.find("Release").each(function () {
        var releaseName = this.parentNode.parentNode.parentNode.lastChild.textContent + "/" + this.textContent;
        versionArray.push(releaseName);
    });

    return versionArray;
}

function PopulateVersionsSelect(id, versionInfo) {
    $(id).empty();
    $(id).append("<option>--Select--</option>");
    $(versionInfo).each(function (i, text) {
        $(id).append("<option value=" + text + ">" + text + "</option>");
    });
}

function GetChanges() {
    showLoad();
    $.ajax({
        url: "http://localhost:51458/Home/GenerateReport?first=" + $("#Version1 option:selected").val() + "&second=" + $("#Version2 option:selected").val(),
        dataType: 'JSON',
        error: function () {
            showPage();
        },
        success: function (data) {
            JsonTree = data;
            $("#tree").fancytree('getTree').reload(JsonTree);
            $("#diff_frame").contents().find('html').html("");
            showPage();
        }
    });
}

function GetDiffHtml(path) {
    showLoad();
    $.ajax({
        url: "http://localhost:51458/Home/GetDiffHtml?first=" + $("#Version1 option:selected").val() + "&second=" + $("#Version2 option:selected").val() + "&filePath=" + path,
        error: function () {
            showPage();
        },
        success: function (data) {
            $("#diff_frame").contents().find('html').html(data);
            showPage();
        }
    });
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

function IsFile(treeNode) {
    var isFile = false;

    if (treeNode) {
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