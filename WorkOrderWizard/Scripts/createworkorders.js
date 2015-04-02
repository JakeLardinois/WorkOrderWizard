$(document).ready(function () {
    var objWOTypes = WOTypes();
    var objWOEquipmentList = WOEquipmentList();


    

    $("#PersonalSwitcher").themeswitcher({
        imgpath: sImagesURL,
        loadTheme: "dot-luv"
    });

    $("input[type=submit], a, button")
      .button()
      .click(function (event) {

          var statesdemo = {
              state0: {
                  title: 'Work Order Wizard - Step 1',
                  html: '<form class="WorkOrderWizard" id="WorkOrderWizardStep1"><div class="error"></div>' +
                      '<br><select title="Select Priority" id="WOPriority" multiple="false" required needsSelection></select>' +
                      '<br><select title="Select Type" id="WOTypes" multiple="false" required needsSelection></select>' +
                      '<br><input title="Enter Employee #..." id="EmployeeNo" type="text" placeholder="Enter Employee #..." required><label id="EmployeeName"></label><input id="EmployeeInfo" type="hidden">' +
                      '<br><textarea title="Enter Work Order Notes..." id="WONotes" placeholder="Enter Work Order Notes..." required></textarea>' +
                      '</form>',
                  buttons: { Next: 1, Cancel: -1 },
                  submit: function (e, v, m, f) {
                      if (v == -1) {}
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
                  html: '<p class="message success">Chose up to 5 pieces of Equipment.</p>' +
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
                      else if (v == 0) {}
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
                                  TASKDESC: $('#WONotes').val(),
                                  WOPriority: $('#WOPriority').val()
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
              $.post(sSLEmployeeLookupURL + '?&EmployeeID=' + this.value , {}, function (data) {
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
                  if ($(this).multiselect("widget").find("input:checked").length > 5) {
                      warning.addClass("error").removeClass("success").html("You can only chose a MAX of 5 pieces of Equipment!");
                      return false;
                  } else {
                      warning.addClass("success").removeClass("error").html("Chose up to 5 pieces of Equipment.");
                  }
              }
          }).multiselectfilter();
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
