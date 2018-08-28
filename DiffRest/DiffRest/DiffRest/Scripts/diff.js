function GetDiff() {
    $.ajax({
        url: "/Home/List",
        data: {
            "oldRelease": document.getElementById("old").value,
            "newRelease": document.getElementById("new").value,
            "noChange": document.getElementById("CheckNoChange").checked,
            "eddited": document.getElementById("CheckEdited").checked,
            "added": document.getElementById("CheckAdded").checked,
            "removed": document.getElementById("CheckRemoved").checked
        },
        type: "GET",
        success: function (msg) {
            alert("Please wait, this may take some time.");
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