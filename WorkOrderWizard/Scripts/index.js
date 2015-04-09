$(document).ready(function () {
    
    $("#PersonalSwitcher").themeswitcher({
        imgpath: sImagesURL,
        loadTheme: "dot-luv"
    });

    $("#ViewWOMenuBtn").button()
    $("#CreateWOMenuBtn").button()
});


function WOTypes() {
    $.ajaxSetup({ async: false, dataType: "json" });

    $.getJSON(sWOTypesURL, {}, function (data) {
        WOTypes = FormatWOTypeSelectColumnJSON(data);
    });
    $.ajaxSetup({ async: true }); //Sets ajax back up to synchronous

    return WOTypes;
}

function FormatWOTypeSelectColumnJSON(x) {
    var finalEdit = new Array();

    //Loop through the list
    for (i = 0; i < x.length; i++) {
        //because I use getJSON when doing a GET in my Ajax call, I have strongly typed JSON objects to use below...
        finalEdit[i] = { value: x[i].WOTYPE, label: x[i].DESCRIPTION };
    }

    return finalEdit;
}

function WOEquipmentList() {
    $.ajaxSetup({ async: false, dataType: "json" });

    $.getJSON(sWOEquipmentListURL, {}, function (data) {
        WOEquipmentList = FormatWOEquipmentSelectColumnJSON(data);
    });
    $.ajaxSetup({ async: true }); //Sets ajax back up to synchronous

    return WOEquipmentList;
}

function FormatWOEquipmentSelectColumnJSON(x) {
    var finalEdit = new Array();

    //Loop through the list
    for (i = 0; i < x.length; i++) {
        //because I use getJSON when doing a GET in my Ajax call, I have strongly typed JSON objects to use below...
        finalEdit[i] = { value: x[i].EQNUM, label: x[i].DescriptionDisplay };
    }

    return finalEdit;
}

