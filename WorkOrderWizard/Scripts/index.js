var anOpen = [];
var oTable;
var intMaxRecordCount = 1000;
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
    "fnClick": null,
    /*"fnClick": function (nButton, oConfig) {
        var oParams = this.s.dt.oApi._fnAjaxParameters(this.s.dt);
        var iframe = document.createElement('iframe');
        iframe.style.height = "0px";
        iframe.style.width = "0px";
        oParams.push({ "name": "MaxRecordCount", "value": 0 }); //maxrecordcount isn't used on download report...
        oParams.push({ "name": "isDownloadReport", "value": true });
        iframe.src = oConfig.sUrl + "?" + $.param(oParams);
        document.body.appendChild(iframe);
    },*/
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

    $("#SearchButton")
      .button()
      .click(function (event) {
          oTable.draw();
      });


    //$("input[type=submit], a, button")
    $("#WOButton")
      .button()
      .click(function (event) {

          var statesdemo = {
              state0: {
                  title: 'Work Order Wizard - Step 1',
                  html: '<form class="WorkOrderWizard" id="WorkOrderWizardStep1"><div class="error"></div>' +
                      '<br><select title="Select Priority" id="WOPriority" multiple="false" required needsSelection></select>' +
                      '<br><select title="Select Type" id="WOTypes" multiple="false" required needsSelection></select>' +
                      '<br><input title="Enter Employee #..." id="EmployeeNo" type="text" placeholder="Enter Employee #..." required><label id="EmployeeName"></label><input id="EmployeeInfo" type="hidden">' +
                      '<br><input title="Enter Title..." id="TaskDesc" type="text" maxlength="72" placeholder="Enter Title..." required>' +
                      '<br><textarea title="Enter Work Order Notes..." id="WONotes" placeholder="Enter Work Order Notes..." required></textarea>' +
                      '</form>',
                  buttons: { Next: 1, Cancel: -1 },
                  submit: function (e, v, m, f) {
                      if (v == -1) { }
                      else if (v == 1) {
                          $('#WorkOrderWizardStep1').validate({
                              ignore: ':hidden:not("#WOPriority, #WOTypes")', //Tells it to ignore hidden fields except for the ones with id WOPriority and WOTypes
                              //errorClass: 'invalid',
                              errorLabelContainer: $("#WorkOrderWizardStep1 div.error")
                          });

                          e.preventDefault();
                          if ($("#WorkOrderWizardStep1").children("input, select, textarea").valid())
                              $.prompt.goToState('state1');
                      }

                  }
              },
              state1: {
                  title: 'Work Order Wizard - Step 2',
                  html: '<p class="message success">Chose up to 15 pieces of Equipment.</p>' +
                      '<form class="WorkOrderWizard" id="WorkOrderWizardStep2"><div class="error"></div>' +
                      '<select title="Select at least 1 piece of equipment..." id="WOEquipment" multiple="true" required needsSelection></select>' +
                      '</form>',
                  buttons: { Back: -1, Cancel: 0, 'Create Work Order': 1 },
                  focus: 2, //sets the focus onto the Create button...
                  submit: function (e, v, m, f) {
                      if (v == -1) {
                          e.preventDefault();
                          $.prompt.goToState('state0');
                      }
                      else if (v == 0) { }
                      else if (v == 1) {
                          $('#WorkOrderWizardStep2').validate({
                              ignore: ':hidden:not("#WOEquipment")', //Tells it to ignore hidden fields except for the ones with id WOPriority and WOTypes
                              //errorClass: 'invalid',
                              errorLabelContainer: $("#WorkOrderWizardStep2 div.error")
                          });

                          if ($("#WorkOrderWizardStep2").children("select").valid()) {
                              var objData = {
                                  WOType: $('#WOTypes').val(),
                                  WOEquipment: $('#WOEquipment').val(),
                                  EmployeeInfo: $('#EmployeeInfo').val(),
                                  TaskDesc: $('#TaskDesc').val(),
                                  WOPriority: $('#WOPriority').val(),
                                  NOTES: $('#WONotes').val()
                              }

                              $.post(sCreateWOURL, JSON.stringify(objData), function (data) {
                                  if (data.Success) {
                                      $.prompt(data.Message);
                                  }
                                  else {
                                      $.prompt(data.Message);
                                  }
                              });
                          }
                          else {
                              e.preventDefault();
                          }
                      }
                  }
              }
          };

          $.prompt(statesdemo);


          $("#WOPriority").append("<option value=\"1\">Priority 1 (High)</option>");
          $("#WOPriority").append("<option value=\"2\">Priority 2</option>");
          $("#WOPriority").append("<option value=\"3\">Priority 3 (Med)</option>");
          $("#WOPriority").append("<option value=\"4\">Priority 4</option>");
          $("#WOPriority").append("<option value=\"5\">Priority 5 (Low)</option>");
          $("#WOPriority").multiselect({
              multiple: false,
              minWidth: 125,
              header: false,
              noneSelectedText: "Select Priority",
              selectedList: 1 //this is what puts the selected value onto the select box...
          });

          objWOTypes.forEach(function (obj) {
              $("#WOTypes").append("<option value=\"" + obj.value + "\">" + obj.label + "</option>");
          });
          $("#WOTypes").multiselect({
              multiple: false,
              minWidth: 300,
              header: false, // "Select a Work Order Type",
              noneSelectedText: "Select Type",
              selectedList: 1 //this is what puts the selected value onto the select box...
          });

          $("#EmployeeNo").focusout(function () {
              var objEmployee;

              $.ajaxSetup({ async: false, dataType: "json" });
              $.post(sSLEmployeeLookupURL + '?&EmployeeID=' + this.value, {}, function (data) {
                  objEmployee = data;
              }, "json");
              $.ajaxSetup({ async: true }); //Sets ajax back up to synchronous

              if (objEmployee.EmployeeFound) {
                  $("#EmployeeName").text(' ' + objEmployee.fname + ' ' + objEmployee.lname);
                  $("#EmployeeNo").val(objEmployee.emp_num.trim());
                  $("#EmployeeInfo").val(objEmployee.emp_num.trim() + ':' + objEmployee.fname + '_' + objEmployee.lname);
              }
              else {
                  $("#EmployeeName").text('');
                  $("#EmployeeNo").val('');
                  $("#EmployeeInfo").val('');
              }

          })

          $("#WONotes").autogrow();




          var warning = $(".message");
          objWOEquipmentList.forEach(function (obj) {
              $("#WOEquipment").append("<option value=\"" + obj.value + "\">" + obj.label + "</option>");
          });
          $("#WOEquipment").multiselect({
              multiple: true,
              //hide: "explode",
              minWidth: 350,
              noneSelectedText: "Select Equipment",
              selectedText: "# Equipment Selected",
              beforeclose: function () {
                  $("#WorkOrderWizardStep2").children("select").valid();
              },
              beforeopen: function () {
                  $(".ui-multiselect-all").hide(); //hides checkall
                  //$(".ui-multiselect-none").hide();
              },
              open: function () {
                  $(".ui-multiselect-all").hide(); //hides checkall
                  //$(".ui-multiselect-none").hide();
              },
              click: function (e) {
                  if ($(this).multiselect("widget").find("input:checked").length > 15) {
                      warning.addClass("error").removeClass("success").html("You can only chose a MAX of 15 pieces of Equipment!");
                      return false;
                  } else {
                      warning.addClass("success").removeClass("error").html("Chose up to 15 pieces of Equipment.");
                  }
              }
          }).multiselectfilter();
      });


    $("#REQUESTDATEFromFilter").datepicker();
    $("#REQUESTDATEToFilter").datepicker();
    $("#CLOSEDATEFromFilter").datepicker();
    $("#CLOSEDATEToFilter").datepicker();
    $("#COMPLETIONDATEFromFilter").datepicker();
    $("#COMPLETIONDATEToFilter").datepicker();

    $("#STATUSFilter").append("<option selected=\"selected\" value=\"O\">Open</option>");
    $("#STATUSFilter").append("<option value=\"R\">Ready</option>");
    $("#STATUSFilter").append("<option value=\"H\">Hold</option>");
    $("#STATUSFilter").append("<option value=\"C\">Completed</option>");
    //$("#STATUSFilter").append("<option value=\"M\">Unknown</option>");
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
        //hide: "explode",
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
            //I commented the below out in favor of utilizing a button to submit the search due to access being slow on queries...
            /*if (blnCheckChanged) {
                blnCheckChanged = false;
                oTable.draw();
            }*/
        }
    });

    

    $.editable.addInputType('autogrow', {   //adds the autogrow plugin for editing notes
        element: function (settings, original) {
            var textarea = $('<textarea />');
            if (settings.rows) {
                textarea.attr('rows', settings.rows);
            } else {
                textarea.height(settings.height + 50);
            }
            textarea.attr('id', settings.id);

            $(this).append(textarea);
            $(this).append('<br >'); //This is how you get the buttons to be below the textarea box...
            //$(this).append($('<input type="submit" value="Process Me!" >').button()); //this works as well...
            $(this).append($('<button type="submit">Save</button>').button());

            /*I had an issue when I manually added my below cancel button because a default jquery button has a type of submit and i believe that this was overriding the type I specified of cancel
             * so every time I hit my cancel button the jeditable form would get submitted. I could have set onblur=cancel and then set my cancel button below to type=whatever (leaving it as 
             * type=cancel still caused a submit) but I wanted the user to be able to click outside of the textbox when editing and so I had to leave onblur=ignore. The only way that I found to be
             * able to make jeditable call the cancel event was to copy the below code from line 440 in jquery.jeditable.js and change reset.apply(form, [settings, original]); 
             * to reset.apply(this, [settings, original]); and call it on the click event of my custom jquery button in the code below.*/
            $(this).append(
                $('<button type="cancel">Cancel</button>').button()
                .click(function (event) {
                    if ($.isFunction($.editable.types[settings.type].reset)) {
                        var reset = $.editable.types[settings.type].reset;
                    } else {
                        var reset = $.editable.types['defaults'].reset;
                    }
                    reset.apply(this, [settings, original]);
                    return false;
                }));
            return (textarea);
        },
        plugin: function (settings, original) {
            $('textarea', this).autogrow(settings.autogrow);
        }
    });

    oTable = $('#objItems').DataTable({
        "bJQueryUI": true,
        "bProcessing": true,
        "bServerSide": true,
        "bFilter": true,
        //"bPaginate": false,
        "sDom": 'T<"clear">Rlrtip', //The 'R' enables column reorder with resize; UPDATE took out the f from "Rlfrtip" to hide the search textbox, T gives all the buttons
        "oTableTools": {
            sRowSelect: "os", //enables the selection of rows //"multi" enables the selection of multiple rows
            sRowSelector: 'td:first-child', //sets the first column of the row as the one to select the row
            "aButtons": [
                //{
                //    "sExtends": "download",
                //    "sButtonText": "Excel Download",
                //    "sUrl": sGetWorkOrdersUrl, //+ "?&isDownloadReport=True", //+ intMaxRecordCount // "/generate_csv.php"
                //    "sToolTip": "Download an Excel file based on the provided search criteria...",
                //    "fnClick": function (nButton, oConfig) {
                //        var aoData = this.s.dt.oApi._fnAjaxParameters(this.s.dt);

                //        //aoData.push({ "name": "MaxRecordCount", "value": 0 });
                //        //aoData.push({ "name": "isDownloadReport", "value": true });

                //        AppendAdditionalParameters(aoData);

                //        /*I tried to use $.ajax to do an HTTP GET for downloading the excel file, but for some reason I couldn't get my response stream to render the excel file as a download
                //        So I had to issue my HTTP GET using the iframe method below for creating the HTTP GET */
                //        var iframe = document.createElement('iframe');
                //        iframe.style.height = "0px";
                //        iframe.style.width = "0px";
                //        //oParams.push({ "name": "MaxRecordCount", "value": 0 }); //maxrecordcount isn't used on download report...
                //        //oParams.push({ "name": "isDownloadReport", "value": true });
                //        iframe.src = oConfig.sUrl + "?" + $.param(aoData) + "&Format=WOEquipExcel"; //parameterizes the json array aoData and appends it to the URL; HTTP GET standard only allows parameters to be sent via the URL
                //        document.body.appendChild(iframe);
                //        /*$.ajax({  //This is the setting that does the posting of the data...
                //            "dataType": 'json',
                //            "type": "GET",
                //            "url": oConfig.sUrl + "?" + $.param(aoData), //parameterizes the json array oParams (aka aoData) and appends it to the URL...
                //            //"data": aoData
                //            //"success": fnCallback
                //        });*/
                //    },
                //},
                //{
                //    "sExtends": "download",
                //    "sButtonText": "WO Equip CSV Download",
                //    "sUrl": sGetWorkOrdersUrl,
                //    "sToolTip": "Download a CSV file based on the provided search criteria...",
                //    "fnClick": function (nButton, oConfig) {
                //        var aoData = this.s.dt.oApi._fnAjaxParameters(this.s.dt);

                //        AppendAdditionalParameters(aoData);

                //        var iframe = document.createElement('iframe');
                //        iframe.style.height = "0px";
                //        iframe.style.width = "0px";
                //        iframe.src = oConfig.sUrl + "?" + $.param(aoData) + "&Format=WOEquipCSV"; //parameterizes the json array aoData and appends it to the URL; HTTP GET standard only allows parameters to be sent via the URL
                //        document.body.appendChild(iframe);
                //    }
                //},
                {
                    "sExtends": "download",
                    "sButtonText": "Excel Download",
                    "sUrl": sGetWorkOrdersUrl,
                    "sToolTip": "Download a CSV file based on the provided search criteria...",
                    "fnClick": function (nButton, oConfig) {
                        var aoData = this.s.dt.oApi._fnAjaxParameters(this.s.dt);

                        AppendAdditionalParameters(aoData);

                        var iframe = document.createElement('iframe');
                        iframe.style.height = "0px";
                        iframe.style.width = "0px";
                        iframe.src = oConfig.sUrl + "?" + $.param(aoData) + "&Format=WOExcel"; //parameterizes the json array aoData and appends it to the URL; HTTP GET standard only allows parameters to be sent via the URL
                        document.body.appendChild(iframe);
                    }
                },
                {
                    "sExtends": "download",
                    "sButtonText": "CSV Download",
                    "sUrl": sGetWorkOrdersUrl,
                    "sToolTip": "Download an Excel file based on the provided search criteria...",
                    "fnClick": function (nButton, oConfig) {
                        var aoData = this.s.dt.oApi._fnAjaxParameters(this.s.dt);

                        AppendAdditionalParameters(aoData);

                        var iframe = document.createElement('iframe');
                        iframe.style.height = "0px";
                        iframe.style.width = "0px";
                        iframe.src = oConfig.sUrl + "?" + $.param(aoData) + "&Format=WOCSV"; //parameterizes the json array aoData and appends it to the URL; HTTP GET standard only allows parameters to be sent via the URL
                        document.body.appendChild(iframe);
                    }
                }
                //{
                //    "sExtends": "download",
                //    "sButtonText": "Submit Search",
                //    "sUrl": sGetWorkOrdersUrl,
                //    "sToolTip": "Refreshes the below table based on the provided search criteria...",
                //    "fnClick": function (nButton, oConfig) {
                //        var aoData = this.s.dt.oApi._fnAjaxParameters(this.s.dt);

                //        AppendAdditionalParameters(aoData);

                //        var iframe = document.createElement('iframe');
                //        iframe.style.height = "0px";
                //        iframe.style.width = "0px";
                //        oTable.draw();
                //    },
                //},
            ]
        },
        //"sScrollX": "100%",
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
                AppendAdditionalParameters(aoData)

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
                "mDataProp": null,
                //"sWidth": 60,
                "bSortable": false,
                //"bSearchable": false,
                "sClass": "equip center",
                "sDefaultContent": '<img src="' + sOpenImageUrl + '">'
            },
            {
                "mDataProp": null, //Note that I had a problem with this column being first because when the datatable loads, it automatically sorts based on the first column; since this column had a null value
                "sWidth": 60,
                "sClass": "desc center", //applies the control class to the cell and the center class(which center aligns the image)
                "bSortable": false,
                "bSearchable": false,
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
                "mDataProp": "COMPLETIONDATE",
                "render": function (data, type, full, meta) {
                    return FormatDate(data);
                }
            },
            {
                "mDataProp": "COMPLETIONTIME",
                "render": function (data, type, full, meta) {
                    return FormatTime(data);
                }
            }]
    });

    $('#objItems tbody').on('click', 'td.equip', function () {
        var nTr;
        var i;
        var rowIndex;
        var nDetailsRow;
        var sEquipTableName;


        nTr = this.parentNode;
        i = $.inArray(nTr, anOpen);

        rowIndex = oTable.row(nTr).index(); //get the index of the current row
        sEquipTableName = 'dtEquipTable' + rowIndex;

        if (i === -1) { //the datatable is opening the row...
            $('img', this).attr('src', sCloseImageUrl);
            nDetailsRow = oTable.row(nTr).child(GetEquipTableHTML(oTable, nTr, sEquipTableName), 'details').show();
            $('div.innerDetails', nDetailsRow).slideDown();
            anOpen.push(nTr);


            var tInnerTable = $('#' + sEquipTableName).DataTable({ //when referencing the table for it's api, it always wants to be prefaced with #
                //"bProcessing": true,
                "bJQueryUI": true,
                "aaData": oTable.row(rowIndex).data().WOEQLIST,
                "sDom": "Rlfrtip", //Enables column reorder with resize
                "sDom": '<"top">rt<"bottom"flp><"clear">', //hides the filter box and the 'showing recordno of records' message
                "bFilter": false,   //hides the search box
                "bPaginate": false, //disables paging functionality
                //"bServerSide": true,
                //"sAjaxSource": sLineURL + '?&OrderNo=' + sOrderNo, //pass the order number to the orderline url as a querystring; note query string variables are delimited by &
                //"sServerMethod": "POST", 
                "aoColumns": [
                    {
                        "mDataProp": "EQNUM",
                        "sWidth": 60,
                    },
                    {
                        "mDataProp": null, //Note that I had a problem with this column being first because when the datatable loads, it automatically sorts based on the first column; since this column had a null value
                        "sWidth": 60,
                        "sClass": "eqipnotes center", //applies the control class to the cell and the center class(which center aligns the image)
                        "bSortable": false,
                        "bSearchable": false,
                        "sDefaultContent": '<img src="' + sOpenImageUrl + '">'
                    },
                    {
                        "mDataProp": "EQDESC",
                        //"sWidth": 120,
                    },
                    { "mDataProp": "LOCATION" },
                    { "mDataProp": "SUBLOCATION1" },
                    { "mDataProp": "DEPARTMENT" }]

            });
        }
        else { //the datatable is closing the row...
            //$(sEquipTableName).remove(); //I didn't need to remove the datatable once I was dynamically naming them based on row number

            $('img', this).attr('src', sOpenImageUrl);
            $('div.innerDetails', $(nTr).next()[0]).slideUp(function () {
                oTable.row(nTr).child.hide();
                anOpen.splice(i, 1);
            });
        }

        $('#' + sEquipTableName + ' tbody').on('click', 'td.eqipnotes', function () {
            var tr = $(this).closest('tr'); //tr = this.parentNode;
            var row = tInnerTable.row(tr);


            if (row.child.isShown()) {
                // This row is already open - close it
                $('img', this).attr('src', sOpenImageUrl);
                row.child.hide();
                //alert("closing");
                tr.removeClass('shown');
            }
            else {
                // Open this row
                $('img', this).attr('src', sCloseImageUrl);
                var oData = row.data();
                row.child(oData.HTMLTEXTS).show();
                //alert("opening");
                tr.addClass('shown');
            }

        });


    });

    $('#objItems tbody').on('click', 'td.desc', function () {
        var nTr;
        var i;
        var rowIndex;
        var oObj;


        nTr = this.parentNode;
        i = $.inArray(nTr, anOpen);

        rowIndex = oTable.row(nTr).index(); //get the index of the current row
        oObj = oTable.row(rowIndex).data(); //get the WorkOrder object

        sHTML =
        '<table>' +
            '<tbody>' +
                '<tr>' +
                    '<td>' +
                        '<label for="TASKDESC"><b>Description: </b></label>' +
                        '<label id="TASKDESC">' + oObj.TASKDESC + '</label>' +
                    '</td>' +
                '</tr>' +
                '<tr>' +
                    '<td>' +
                        '<div id="WONoteEditorDiv" class="EditWONotes">' +
                            oObj.HTMLWONotes +
                        '</div>' +
                    '</td>' +
                '</tr>' +
            '</tbody>' +
        '</table>';

        $("#DescriptionDialogDiv").html(sHTML);

        $('.EditWONotes').editable(sWONotesUpdateUrl, { //make the note detail editable...
            id: 'WONotes',  //gives the label 'id' the label 'PostedNoteData'
            name: 'NoteContent', //labels the updated data as NoteContent instead of 'value'
            submitdata: { WONUM: oObj.WONUM, CLOSEDATE: oObj.CLOSEDATE, WONotes: oObj.WONotes },
            type: "autogrow", //specifies to use the autogrow input specified above
            //submit: 'OK', //Adds the OK button that submits post UPDATE-I no longer need these since I manually create the buttons in my above "autogrow" creation...
            //cancel: 'Cancel', //adds the Cancel button that cancels the edit
            tooltip: "Click to edit Notes...",
            onblur: "ignore", //what happens when user clicks out of textarea values can be "ignore", "submit", or "cancel"
            indicator: '<img src="' + sProgressImageUrl + '">', //This is what shows while ajax is processing. Could also just be a text message like 'Saving...'
            data: function (value, settings) { //This gets fired when the control goes into edit mode
                /* Convert <br> to newline. */
                var retval = value.replace(/<br[\s\/]?>/gi, '\n');
                return retval;
            },
            callback: function (value, settings) {//This is fired after the control uses ajax to save the data to the server and the JSON Result is recieved back from the server
                sValue = JSON.parse(value); //puts the JSON data into objects form so they can be accessed like sValue.LastUpdate, etc.

                if (sValue.Success) { oTable.draw(); }
                else {
                    alert('The edits failed to save');
                }
                $('#WONoteEditorDiv').html(sValue.HTMLWONotes); //Sets the NoteEditorDiv to the html of the newly saved Notes Object's HTML property
                //window.location.reload();

            }
        });

        // Open this Datatable as a modal dialog box.
        $('#DescriptionDialogDiv').dialog({
            modal: false,
            resizable: true,
            position: 'center',
            width: 'auto', //By not setting the width here, the width stays at whatever the user sets whenever they click on a new address.
            autoResize: true,
            title: 'Work Order ' + oObj.WONUM
        });

    });


    //This is so that you can cause a search to occur on keyup for input field by adding class="dtSearchField" to it's html...
    /*$('input.dtSearchField').on('keyup change', function () {
        oTable.draw(); //forces the table to redraw and the search criteria is set above
    });*/
    //I had to remove the above and create a search button because the Access Database was too slow to handle the multiple queries that occurred when users would type and select options...

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
        //finalEdit[i] = { value: x[i].WOTYPE, label: x[i].DESCRIPTION }; //when I switched to using LinqToCSV, tables with a column that had the same name, had _column appended to them...
        finalEdit[i] = { value: x[i].WOTYPE_Column, label: x[i].DESCRIPTION };
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

function AppendAdditionalParameters(aoData) {
    var strTemp;


    /*I add the FixedColumnHeaders list to the data array that is sent to the server. This is a custom implementation to accomodate the fact that when datatables implements reorderable columns, the sSearch
    * variables get sent to the server based on the old column position whereas mDataProp gets sent based on the new column position. I am then unable to implement a multicolumn search because the data property and
    * search property don't align. So I send this FixedColumnHeaders to the server for use in searching based on the corresponding Ssearch variables. This list must match exactly the columns in DataTables table instantiation. 
    * I had previously implemented this in the server side code, but then any time my UI changed I would need to recompile the web service... So I fixed the implementation...*/
    aoData.push({
        "name": "FixedColumnHeaders",
        "value": ["WONUM", "WOEQLIST", "0", "STATUS", "PRIORITY", "WOTYPE", "ORIGINATOR", "REQUESTDATE", 0, "COMPLETIONDATE"]
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
            case "sSearch_1":
                strTemp = String($('#WOEQLISTFilter').val());
                if (strTemp !== 'null')
                    aoData[i].value = strTemp.split(',').join('|');
                else
                    aoData[i].value = '';
                break;
            case "bRegex_1":
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
                aoData[i].value = $('#COMPLETIONDATEFromFilter').val() + '~' +
                        $('#COMPLETIONDATEToFilter').val();
                break;
            case "bRegex_9":
                aoData[i].value = true;
                break;
            case "sSearch": //I set the sSearch variable so that I can easily grab the value from the controller to filter my initial WO list. UPDATE- No longer implemented...
                strTemp = String($('#STATUSFilter').val());
                if (strTemp !== 'null')
                    aoData[i].value = strTemp;
                else
                    aoData[i].value = 'O'; //by default send the status of O if no status is specified
                break;
        }
    }
}
