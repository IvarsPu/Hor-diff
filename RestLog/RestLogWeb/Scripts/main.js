
$(document).ready(function() {

    loadVersions();

});


function loadVersions() {

    $.getJSON(window.location.pathname + '/api/versions', function (jsonPayload) {
        var activeVersion;

        $(jsonPayload).each(function (i, item) {
            var activeFlag = '';
            if (i == 0) {
                activeFlag = 'active';
                activeVersion = item.name;
            }

            $('#myList').append('<a class="list-group-item list-group-item-action ' + activeFlag + ' data-toggle="list" href="#home" role="tab" onClick="VersionClicked(this)">' + item.name + '</a>');
        });

        if (activeVersion) {
            showVersionJiras(activeVersion);
        }
    });
}

function VersionClicked(owner) {
    $(".list-group-item.active").removeClass('active');
    owner.className += " active";
    $('#release_notes_content').html("&nbsp;");
    showVersionJiras(owner.text);
}

function showVersionJiras(version) {
    var url = window.location.pathname + '/api/versions/' + version + '/filterJiras';

    $('#version_table').bootstrapTable('showLoading');

    $.getJSON(url, function (jsonPayload) {

        $('#version_table').bootstrapTable('hideLoading');
        $('#notification').hide();  
        $('#version_table').bootstrapTable("load", jsonPayload);
    });
}

/*        $('#version_table').bootstrapTable({
data: jsonPayload
        }); */

$(function () {
    var $result = $('#eventsResult');

    $('#version_table').on('click-row.bs.table', function (e, row, $element) {
        $('#release_notes_content').html(row.releaseNotes);
        console.log('Event:', e, ', data:', row);
    }).on('post-body.bs.table', function (e, row, $element) {
        var top = $('.fixed-table-container').position().top;
        $('.col-sm-2').css('padding-top', top);
    }).on('click', '.clickable-row', function (event) {
        $(this).addClass('active').siblings().removeClass('active');
    });

});

//
// Bootstrap table formaters
//
function jira_formater(value, row) {
    return "<a href='" + row.url + "'>" + value + "</a>";
}

function rowStyle(row, index) {

    if (row.key == "") {
        return {
            classes: 'text-nowrap',
            css: { "background-color": "#f5f5f5" }
        };
    } else {
        return {
            classes: 'clickable-row',
            css: {  }
        };
    }

}


