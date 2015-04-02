var anOpen = [];
var oVRequestTable;
var intMaxRecordCount = 200;
var blnCheckChanged;


TableTools.BUTTONS.download = {
    "sAction": "text",
    "sTag": "default",
    "sFieldBoundary": "",
    "sFieldSeperator": "\t",
    "sNewLine": "<br>",
    "sToolTip": "",
    "sButtonClass": "DTTT_button_text",
    "sButtonClassHover": "DTTT_button_text_hover",
    "sButtonText": "Download",
    "mColumns": "all",
    "bHeader": true,
    "bFooter": true,
    "sDiv": "",
    "fnMouseover": null,
    "fnMouseout": null,
    "fnClick": function (nButton, oConfig) {
        var oParams = this.s.dt.oApi._fnAjaxParameters(this.s.dt);
        var iframe = document.createElement('iframe');
        iframe.style.height = "0px";
        iframe.style.width = "0px";
        oParams.push({ "name": "MaxRecordCount", "value": intMaxRecordCount });
        iframe.src = oConfig.sUrl + "?" + $.param(oParams);
        document.body.appendChild(iframe);
    },
    "fnSelect": null,
    "fnComplete": null,
    "fnInit": null
};

$(document).ready(function () {
    var objWOTypes = WOTypes();
    var objWOEquipmentList = WOEquipmentList();
    var oTimerId;


    $("#PersonalSwitcher").themeswitcher({
        imgpath: sImagesURL,
        loadTheme: "dot-luv"
    });

    $("#REQUESTDATEFromFilter").datepicker();
    $("#REQUESTDATEToFilter").datepicker();
    $("#CLOSEDATEFromFilter").datepicker();
    $("#CLOSEDATEToFilter").datepicker();

    $("#STATUSFilter").append("<option value=\"O\">Open</option>");
    $("#STATUSFilter").append("<option value=\"R\">Ready</option>");
    $("#STATUSFilter").append("<option value=\"H\">Hold</option>");
    $("#STATUSFilter").append("<option value=\"C\">Completed</option>");
    $("#STATUSFilter").multiselect({
        multiple: true,
        hide: "explode",
        minWidth: 125,
        //header: false,
        noneSelectedText: "Select Status",
        //selectedList: 1 //this is what puts the selected value onto the select box...
    });

    $("#PRIORITYFilter").append("<option value=\"1\">Priority 1 (High)</option>");
    $("#PRIORITYFilter").append("<option value=\"2\">Priority 2</option>");
    $("#PRIORITYFilter").append("<option value=\"3\">Priority 3 (Med)</option>");
    $("#PRIORITYFilter").append("<option value=\"4\">Priority 4</option>");
    $("#PRIORITYFilter").append("<option value=\"5\">Priority 5 (Low)</option>");
    $("#PRIORITYFilter").append("<option value=\"0\">Priority 0 (None)</option>");
    $("#PRIORITYFilter").multiselect({
        multiple: true,
        hide: "explode",
        minWidth: 125,
        //header: false,
        noneSelectedText: "Select Priority",
        //selectedList: 1 //this is what puts the selected value onto the select box...
    });

    objWOTypes.forEach(function (obj) {
        $("#WOTYPEFilter").append("<option value=\"" + obj.value + "\">" + obj.label + "</option>");
    });
    $("#WOTYPEFilter").multiselect({
        multiple: true,
        hide: "explode",
        minWidth: 300,
        //header: false, // "Select a Work Order Type",
        noneSelectedText: "Select Type",
        //selectedList: 1 //this is what puts the selected value onto the select box...
    });

    objWOEquipmentList.forEach(function (obj) {
        $("#WOEQLISTFilter").append("<option value=\"" + obj.value + "\">" + obj.label + "</option>");
    });
    $("#WOEQLISTFilter").multiselect({
        multiple: true,
        //hide: "explode", //When using multiselectfilter(), the explode animation doesn't work so I commented it out here...
        minWidth: 350,
        noneSelectedText: "Select Equipment",
        //selectedText: "# Equipment Selected",
        //selectedList: 1 //this is what puts the selected value onto the select box...
    }).multiselectfilter();

    /*I wanted to avoid a query in the situation where a user just opens the select box to observe the checked values. So I created a global variable that gets set only when a user
     * checks/unchecks an option or CheckAll or UnCheckAll.*/
    $("select").multiselect({ //makes all the selects multiselect boxes AND applies the specified methods...
        hide: "explode",
        checkAll: function () {
            blnCheckChanged = true;
        },
        uncheckAll: function () {
            blnCheckChanged = true;
        },
        click: function (event, ui) {
            blnCheckChanged = true;
        },
        close: function () {
            if (blnCheckChanged) {
                blnCheckChanged = false;
                oVRequestTable.draw();
            }
        }
    });

    oVRequestTable = $('#objItems').DataTable({
        "bJQueryUI": true,
        "bProcessing": true,
        "bServerSide": true,
        "bFilter": true,
        "sDom": '<"clear">RlrtTip', //The 'R' enables column reorder with resize; UPDATE took out the f from "Rlfrtip" to hide the search textbox
        "oTableTools": {
            sRowSelect: "os", //enables the selection of rows //"multi" enables the selection of multiple rows
            sRowSelector: 'td:first-child', //sets the first column of the row as the one to select the row
            "aButtons": [
                {
                    "sExtends": "download",
                    "sButtonText": "Excel Download",
                    "sUrl": sPrintWorkOrdersUrl //+ "?MaxRecordCount=" + intMaxRecordCount // "/generate_csv.php"
                }
            ]
        },
        "sScrollX": "100%",
        "oColReorder": {
            "iFixedColumns": 3 //specifies that the first n columns are not reorderable...
        },
        "sPaginationType": "full_numbers",
        "sAjaxSource": sGetWorkOrdersUrl + '?&MaxRecordCount=' + intMaxRecordCount,// document.URL,
        "sServerMethod": "POST",
        "fnServerData": function (sSource, aoData, fnCallback, oSettings) {

            window.clearTimeout(oTimerId); //clear the timer if it still exists
            oTimerId = window.setTimeout(function () {
                var strTemp;


                //aoData.push({ "name": "MaxRecordCount", "value": intMaxRecordCount }); //adds the MaxRecordCount data to the array sent to the server...

                /*I add the FixedColumnHeaders list to the data array that is sent to the server. This is a custom implementation to accomodate the fact that when datatables implements reorderable columns, the sSearch
                 * variables get sent to the server based on the old column position whereas mDataProp gets sent based on the new column position. I am then unable to implement a multicolumn search because the data property and
                 * search property don't align. So I send this FixedColumnHeaders to the server for use in searching based on the corresponding Ssearch variables. This list must match exactly the columns in DataTables table instantiation. 
                 * I had previously implemented this in the server side code, but then any time my UI changed I would need to recompile the web service... So I fixed the implementation...*/
                aoData.push({
                    "name": "FixedColumnHeaders",
                    "value": ["WONUM", "0", "WOEQLIST", "STATUS", "PRIORITY", "WOTYPE", "ORIGINATOR", "REQUESTDATE", 0, "CLOSEDATE"]
                });

                /*iterates through the array and updates the appropriate object using the below 'case' statements. I was having an issue where sSearch was getting populated twice (ie sSearch_4 & sSearch_7 would contain the same search string) 
                 * I solved the issue by manually setting sSearch in my aoData array below. Note that I populate the corresponding bRegex variable; this value isn't used anywhere but could be for future implementations...*/
                for (var i = 0; i < aoData.length; i++) {
                    switch (aoData[i].name) {
                        case "sSearch_0":
                            aoData[i].value = $('#WONUMFilter').val();
                            break;
                        case "bRegex_0":
                            aoData[i].value = true;
                            break;
                        case "sSearch_2":
                            strTemp = String($('#WOEQLISTFilter').val());
                            if (strTemp !== 'null')
                                aoData[i].value = strTemp.split(',').join('|');
                            else
                                aoData[i].value = '';
                            break;
                        case "bRegex_2":
                            aoData[i].value = true;
                            break;
                        case "sSearch_3":
                            strTemp = String($('#STATUSFilter').val());
                            if (strTemp !== 'null')
                                aoData[i].value = strTemp.split(',').join('|');
                            else
                                aoData[i].value = '';
                            break;
                        case "bRegex_3":
                            aoData[i].value = true;
                            break;
                        case "sSearch_4":
                            strTemp = String($('#PRIORITYFilter').val());
                            if (strTemp !== 'null')
                                aoData[i].value = strTemp.split(',').join('|');
                            else
                                aoData[i].value = '';
                            break;
                        case "bRegex_4":
                            aoData[i].value = true;
                            break;
                        case "sSearch_5":
                            strTemp = String($('#WOTYPEFilter').val());
                            if (strTemp !== 'null')
                                aoData[i].value = strTemp.split(',').join('|');
                            else
                                aoData[i].value = '';
                            break;
                        case "bRegex_5":
                            aoData[i].value = true;
                            break;
                        case "sSearch_6":
                            aoData[i].value = $('#ORIGINATORFilter').val();
                            break;
                        case "bRegex_6":
                            aoData[i].value = true;
                            break;
                        case "sSearch_7":
                            aoData[i].value = $('#REQUESTDATEFromFilter').val() + '~' +
                                    $('#REQUESTDATEToFilter').val();
                            break;
                        case "bRegex_7":
                            aoData[i].value = true;
                            break;
                        case "sSearch_9":
                            aoData[i].value = $('#CLOSEDATEFromFilter').val() + '~' +
                                    $('#CLOSEDATEToFilter').val();
                            break;
                        case "bRegex_9":
                            aoData[i].value = true;
                            break;
                    }
                }

                oSettings.jqXHR = $.ajax({  //This is the setting that does the posting of the data...
                    "dataType": 'json',
                    "type": "POST",
                    "url": sSource,
                    "data": aoData, 
                    "success": fnCallback
                });
            }, 1000); //wait 1000 milliseconds (1 sec) before executing the function...
        },
        //"oLanguage": { "sSearch": "Search Order #s:" },
        "aoColumns": [
            { "mDataProp": "WONUM" },
            {
                "mDataProp": null, //Note that I had a problem with this column being first because when the datatable loads, it automatically sorts based on the first column; since this column had a null value
                orderable: false, //it would pass that null value to the data call. I actually fixed this by modifying the code in InMemoryRepositories.cs so that if there was an error, it just returns the list
                "sClass": "control center",
                "sDefaultContent": '<img src="' + sOpenImageUrl + '">'
            },
            {
                "mDataProp": null, //Note that I had a problem with this column being first because when the datatable loads, it automatically sorts based on the first column; since this column had a null value
                orderable: false, //it would pass that null value to the data call. I actually fixed this by modifying the code in InMemoryRepositories.cs so that if there was an error, it just returns the list
                "sClass": "control center",
                "sDefaultContent": '<img src="' + sOpenImageUrl + '">'
            },
            { "mDataProp": "STATUS" },
            { "mDataProp": "PRIORITY" },
            { "mDataProp": "WOTYPE" },
            { "mDataProp": "ORIGINATOR" },
            {
                "mDataProp": "REQUESTDATE",
                "render": function (data, type, full, meta) {
                    return FormatDate(data);
                }
            },
            {
                "mDataProp": "REQUESTTIME",
                "render": function (data, type, full, meta) {
                    return FormatTime(data);
                }
            },
            {
                "mDataProp": "CLOSEDATE",
                "render": function (data, type, full, meta) {
                    return FormatDate(data);
                }
            }]
    });

    //This is so that you can cause a search to occur on keyup for input field by adding class="dtSearchField" to it's html...
    $('input.dtSearchField').on('keyup change', function () {
        oVRequestTable.draw(); //forces the table to redraw and the search criteria is set above
    });
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