function GetDiff() {
    $.ajax({
        url: "/Home/List",
        data: {
            "oldRelease": document.getElementById("oldVersion").value + "/" + document.getElementById("oldRelease").value,
            "newRelease": document.getElementById("newVersion").value + "/" + document.getElementById("newRelease").value,
            "noChange": document.getElementById("CheckNoChange").checked,
            "eddited": document.getElementById("CheckEdited").checked,
            "added": document.getElementById("CheckAdded").checked,
            "removed": document.getElementById("CheckRemoved").checked
        },
        type: "GET",
        success: function (msg) {
            alert("Please wait, this could take some time.");
            $("#tblDiff tbody tr").remove();
            $.each(msg, function (index, item) {
                var tr = $("<tr></tr>");
                tr.html(("<td>" + item.Name + "</td>")
                    + " " + ("<td>" + item.ServiceName + "</td>")
                    + " " + ("<td>" + item.State + "</td>"));
                $("#tblDiff").append(tr);
            });
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}

function GetRelease(obj) {
    $.ajax({
        url: "/Home/Release",
        data: {
            "versionName": obj.value
        },
        type: "GET",
        success: function (msg) {
            var text = "";
            $.each(msg, function (index, item) {
                text += "<option value=" + item + ">" + item + "</option>";
            });
            document.getElementById(obj.id.substring(0, 3) + "Release").innerHTML = text;
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}