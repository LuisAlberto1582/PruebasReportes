var DSOControls = {
    Autocomplete: {},
    DropDown: {},
    Expandable: {},
    TextBox: {},
    ListBox: {},
    DateTimeBox: {},
    Button: {},
    RadioButtonList: {},
    CheckBoxList: {},
    Flags: {},
    NumberEdit: {},
    Window: {},
    Tabs: {},
    Grid: {},
    LoadFunction: function(funcStr) {
        var ret = (typeof funcStr == 'string') ?
            eval('(' + funcStr + ')') :
            funcStr;
        if (ret !== undefined) {
            return ret;
        }
        else {
            return eval('[' + funcStr + '][0]');
        }
    },
    CleanContainer: function() {
        $(this).find("[dataField]").each(function() {
            var $ctl = $(this);
            if ($ctl.hasClass("DSOTextBox")) {
                $ctl.val("");
                $ctl.change();
            }
            else if ($ctl.hasClass("DSODropDownList")) {
                this.selectedIndex = 0;
                $ctl.change();
            }
            else if ($ctl.hasClass("DSODateTimeBox")) {
                $ctl.val("");
                $($ctl.attr('TextValue')).val("");
                $ctl.change();
            }
            else if ($ctl.hasClass("DSOAutocomplete")) {
                $ctl.val("");
                $($ctl.attr('textValueID')).val("");
                $ctl.change();
            }
            else if ($ctl.hasClass("DSOFlags") || $ctl.hasClass("DSOCheckBoxList")) {
                DSOControls.CheckBoxList.Check(this, false);
                $ctl.change();
            }
        });
    },
    LoadContainer: function(row) {
        for (var field in row) {
            var $ctl = $(this).find("[dataField='" + field + "']");

            if ($ctl.hasClass("DSOTextBox") || $ctl.hasClass("DSODropDownList")) {
                $ctl.val(row[field]);
                $ctl.change();
            }
            else if ($ctl.hasClass("DSODateTimeBox")) {
                if (row[field] === null || row[field] === "") {
                    $ctl.val("");
                    $($ctl.attr('TextValue')).val("");
                }
                else {
                    var date = (typeof row[field] == 'string') ?
                            new Date(parseInt(row[field].substr(6))) :
                            row[field];
                    $ctl.datetimepicker('setDate', date);
                }
                $ctl.change();
            }
            else if ($ctl.hasClass("DSOAutocomplete")) {
                if (row[field + "Display"] !== undefined) {
                    $ctl.val(row[field + "Display"]);
                }
                else {
                    $ctl.val(row[field]);
                }
                $($ctl.attr('textValueID')).val(row[field]);
                $ctl.change();
            }
            else if ($ctl.hasClass("DSOFlags")) {
                var value = parseInt(row[field]);

                if (isNaN(value)) {
                    value = 0;
                }
                var itmValue;
                row[field] = value;
                $ctl.prev(".DSOFlagsVal").val(row[field]);
                $ctl.find("input:checkbox").each(function() {
                    //itmValue = Math.pow(2, parseInt($(this).closest('span').attr('data-value')));
                    itmValue = parseInt($(this).closest('span').attr('data-value'));
                    if ((row[field] & itmValue) == itmValue) {
                        this.checked = true;
                    }
                    else {
                        this.checked = false;
                    }
                });
                $ctl.change();
            }
        }
    },
    LoadContainerAjax: function(data, textStatus, jqXHR) {
        var rows = DSOControls.LoadFunction(data.d);
        var row;
        if (rows.length !== undefined && rows.length > 0) {
            row = rows[0];
            DSOControls.LoadContainer.call(this, row);
        }
    },
    ErrorAjax: function(jqXHR, textStatus, errorThrown) {
        var msg = "";
        var error;
        try {
            error = (typeof jqXHR.responseText) == 'string' ?
                        eval('(' + jqXHR.responseText + ')') :
                        jqXHR.responseText;
            msg = error.Message !== undefined && error.Message !== "" ? error.Message : errorThrown;
        }
        catch (ex) {
            msg = errorThrown;
        }
        if (error !== undefined
        && error.ExceptionType !== undefined
        && error.ExceptionType === "KeytiaWeb.KeytiaWebSessionException"
        && msg) {
            jAlert(msg, null, function() {
                __doPostBack("", "");
            });
        }
        else if (error !== undefined
        && error.ExceptionType !== undefined
        && error.ExceptionType === "System.InvalidOperationException") {
            __doPostBack("", "");        
        }
        else if (msg)
            jAlert(msg);
        else
            __doPostBack("", "");
    }
};

//DropDown-------------------------------------------------------------------------

DSOControls.DropDown.Fill = function(data, textStatus, jqXHR) {
    var options = (typeof data.d) == 'string' ?
                    eval('(' + data.d + ')') :
                    data.d;
    var option;
    var maxLength = options.length;
    var value = $(this).val();
    var idx = -1;
    var idxSelect = 0;
    var multiple = $.isArray(value);

    this.options.length = 0;
    if ($(this).attr("selectItemText") != undefined && $(this).attr("selectItemText") != "") {
        //        option = document.createElement("OPTION");
        //        option.value = $(this).attr("selectItemValue");
        //        option.text = $(this).attr("selectItemText")
        //this.add(option, null);
        option = $("<option></option>");
        option.val($(this).attr("selectItemValue"));
        option.html($(this).attr("selectItemText"));
        $(this).append(option);
        this.selectedIndex = 0;
        idxSelect = 1;
    }

    for (var i = 0; i < maxLength; i++) {
        //        option = document.createElement("OPTION");
        //        option.value = options[i].value;
        //        option.text = options[i].text;
        //        //this.add(option, null);
        option = $("<option></option>");
        option.val(options[i].value);
        option.html(options[i].text);
        $(this).append(option);

        if (!multiple && option.val() == value) {
            idx = i;
        }
        else if (multiple && $.inArray(option.val(), value) >= 0) {
            option.attr("selected", "selected");
        }
    }

    if (!multiple && idx != -1) {
        this.selectedIndex = idx + idxSelect;
    }
    else if (!multiple && value !== $(this).val()) {
        this.selectedIndex = 0;
        $(this).change();
    }
    $(this).attr("loaded", true);
}

DSOControls.DropDown.Change = function() {
    $($(this).next(".DSODropDownListVal")).val($(this).children("option:selected").val());
    $($(this).next(".DSODropDownListVal")).change();
}

//TextBox-------------------------------------------------------------------------

DSOControls.TextBox.CheckMaxLength = function() {
    var limit = parseInt($(this).attr('maxlength'));
    var text = $(this).val();
    var chars = text.length;

    if (chars > limit) {
        var new_text = text.substr(0, limit);
        $(this).val(new_text);
    }
}

//ListBox-------------------------------------------------------------------------

DSOControls.ListBox.Change = function() {
    var valores = $(this).val();
    var valor;
    if ($.isArray(valores))
        valor = valores.join($(this).attr("Separator"));
    else
        valor = valores;
    $($(this).attr("TextValue")).val(valor);
}

//DateTimeBox-------------------------------------------------------------------------

DSOControls.DateTimeBox.Init = function() {
    $('.DSODateTimeBox').each(function() {
        var options = {};
        if ($(this).attr('dateFormat') != undefined)
            options.dateFormat = $(this).attr('dateFormat');
        if ($(this).attr('timeFormat') != undefined)
            options.timeFormat = $(this).attr('timeFormat');
        if ($(this).attr('isDisabled') != undefined)
            options.disabled = eval($(this).attr('isDisabled'));
        if ($(this).attr('autoSize') != undefined)
            options.autoSize = eval($(this).attr('autoSize'));
        if ($(this).attr('isRTL') != undefined)
            options.isRTL = eval($(this).attr('isRTL'));
        if ($(this).attr('showMonthAfterYear') != undefined)
            options.showMonthAfterYear = eval($(this).attr('showMonthAfterYear'));
        if ($(this).attr('showWeek') != undefined)
            options.showWeek = eval($(this).attr('showWeek'));

        if ($(this).attr('showHour') != undefined) {
            options.showHour = eval($(this).attr('showHour'));
            if (!options.showHour) options.alwaysSetTime = false;
        }
        if ($(this).attr('showMinute') != undefined) {
            options.showMinute = eval($(this).attr('showMinute'));
            if (!options.showMinute) options.alwaysSetTime = false;
        }
        if ($(this).attr('showSecond') != undefined) {
            options.showSecond = eval($(this).attr('showSecond'));
            if (options.showSecond) options.alwaysSetTime = true;
        }

        if ($(this).attr('ampm') != undefined)
            options.ampm = eval($(this).attr('ampm'));
        if ($(this).attr('timeOnly') != undefined)
            options.timeOnly = eval($(this).attr('timeOnly'));

        if ($(this).attr('firstDay') != undefined)
            options.firstDay = eval($(this).attr('firstDay'));
        if ($(this).attr('numberOfMonths') != undefined)
            options.numberOfMonths = eval($(this).attr('numberOfMonths'));
        if ($(this).attr('showCurrentAtPos') != undefined)
            options.showCurrentAtPos = eval($(this).attr('showCurrentAtPos'));
        if ($(this).attr('stepMonths') != undefined)
            options.stepMonths = eval($(this).attr('stepMonths'));
        if ($(this).attr('stepHour') != undefined)
            options.stepHour = eval($(this).attr('stepHour'));
        if ($(this).attr('stepMinute') != undefined)
            options.stepMinute = eval($(this).attr('stepMinute'));
        if ($(this).attr('stepSecond') != undefined)
            options.stepSecond = eval($(this).attr('stepSecond'));
        if ($(this).attr('hourGrid') != undefined)
            options.hourGrid = eval($(this).attr('hourGrid'));
        if ($(this).attr('minuteGrid') != undefined)
            options.minuteGrid = eval($(this).attr('minuteGrid'));
        if ($(this).attr('secondGrid') != undefined)
            options.secondGrid = eval($(this).attr('secondGrid'));

        if ($(this).attr('appendText') != undefined)
            options.appendText = $(this).attr('appendText');
        if ($(this).attr('prevText') != undefined)
            options.prevText = $(this).attr('prevText');
        if ($(this).attr('nextText') != undefined)
            options.nextText = $(this).attr('nextText');
        if ($(this).attr('weekHeader') != undefined)
            options.weekHeader = $(this).attr('weekHeader');
        if ($(this).attr('yearRange') != undefined)
            options.yearRange = $(this).attr('yearRange');
        if ($(this).attr('yearSuffix') != undefined)
            options.yearSuffix = $(this).attr('yearSuffix');
        if ($(this).attr('timeOnlyTitle') != undefined)
            options.timeOnlyTitle = $(this).attr('timeOnlyTitle');
        if ($(this).attr('hourText') != undefined)
            options.hourText = $(this).attr('hourText');
        if ($(this).attr('minuteText') != undefined)
            options.minuteText = $(this).attr('minuteText');
        if ($(this).attr('secondText') != undefined)
            options.secondText = $(this).attr('secondText');

        if ($(this).attr('minDateTime') != undefined) {
            var minDateTime = $(this).attr('minDateTime');
            var date = new Date(minDateTime.split("|")[0], minDateTime.split("|")[1] - 1, minDateTime.split("|")[2], minDateTime.split("|")[3], minDateTime.split("|")[4], minDateTime.split("|")[5], 0);
            options.minDateTime = date;
        }
        if ($(this).attr('maxDateTime') != undefined) {
            var maxDateTime = $(this).attr('maxDateTime');
            var date = new Date(maxDateTime.split("|")[0], maxDateTime.split("|")[1] - 1, maxDateTime.split("|")[2], maxDateTime.split("|")[3], maxDateTime.split("|")[4], maxDateTime.split("|")[5], 0);
            options.maxDateTime = date;
        }

        if ($(this).attr('dayNames') != undefined)
            options.dayNames = eval($(this).attr('dayNames'));
        if ($(this).attr('dayNamesShort') != undefined)
            options.dayNamesShort = eval($(this).attr('dayNamesShort'));
        if ($(this).attr('dayNamesMin') != undefined)
            options.dayNamesMin = eval($(this).attr('dayNamesMin'));
        if ($(this).attr('monthNames') != undefined)
            options.monthNames = eval($(this).attr('monthNames'));
        if ($(this).attr('monthNamesShort') != undefined)
            options.monthNamesShort = eval($(this).attr('monthNamesShort'));

        if ($(this).attr('showCalendar') != undefined)
            options.showCalendar = eval($(this).attr('showCalendar'));
        if ($(this).attr('showCurrent') != undefined)
            options.showCurrent = eval($(this).attr('showCurrent'));
        if ($(this).attr('alertFormat') != undefined)
            options.alertFormat = $(this).attr('alertFormat');

        var funcArray = [];
        if ($(this).attr('seleccion') != undefined) {
            funcArray.push(DSOControls.LoadFunction($(this).attr('seleccion')));
        }
        if ($(this).attr('postBackOnSelect') != undefined) {
            funcArray.push(DSOControls.LoadFunction($(this).attr('postBackOnSelect')));
        }
        if (funcArray.length > 0) {
            options.onSelect = function(dateText, inst) {
                var idx;
                var maxIdx = funcArray.length;
                for (idx = 0; idx < maxIdx; idx++) {
                    funcArray[idx].call(this, dateText, inst);
                }
            }
        }

        if ($(this).attr('buttonImage') != undefined && ($(this).attr('disabled') === undefined || $(this).attr('disabled') === false)) {
            options.buttonImage = $(this).attr('buttonImage');
            options.buttonImageOnly = true;
            options.showOn = "both";
        }
        if (!options.showCalendar) {
            $(this).attr('disabled', true);
        }

        options.altField = $(this).attr('TextValue');
        options.altFormat = 'yy-mm-dd';
        options.altFieldTimeOnly = false;
        options.changeMonth = true;
        options.changeYear = true;
        options.constrainInput = true;
        options.showTime = false;
        options.showButtonPanel = true;
        
        $(this).datetimepicker(options);
        if ($($(this).attr('TextValue')).val() !== "") {
            var date = new Date($($(this).attr('TextValue')).val().replace(/-/g, "/"));
            if (options.alwaysSetTime != undefined && !options.alwaysSetTime) {
                $(this).datetimepicker('setDate', date);
            }
            else {
                $(this).datetimepicker('setDate', new Date(Date.UTC(
                    date.getFullYear(),
                    date.getMonth(),
                    date.getDate(),
                    date.getHours(),
                    date.getMinutes(),
                    date.getSeconds(),
                    date.getMilliseconds()
                )));
            }
        }
        else {
            $(this).attr("value", "");
        }

        var funcArrayMonth = [];
        if ($(this).attr('onChangeMonthYear') != undefined) {
            funcArrayMonth.push(DSOControls.LoadFunction($(this).attr('onChangeMonthYear')));
        }
        if ($(this).attr('postBackOnChangeMonthYear') != undefined) {
            funcArrayMonth.push(DSOControls.LoadFunction($(this).attr('postBackOnChangeMonthYear')));
        }
        if (funcArrayMonth.length > 0) {
            options.onChangeMonthYear = function(year, month, inst) {
                var idxM;
                var maxIdxM = funcArrayMonth.length;
                for (idxM = 0; idxM < maxIdxM; idxM++) {
                    funcArrayMonth[idxM].call(this, year, month, inst);
                }
            }
            $(this).datetimepicker(options);
        }
    });
}

DSOControls.DateTimeBox.setMonthDay = function(year, month, inst) {
    var $ctl = $("#" + inst.id);
    if ($ctl.attr("monthDayValue") == undefined)
        return;

    var day = parseInt($ctl.attr("monthDayValue"));
    var d = new Date(year, month - 1, day);
    if (d.getMonth() === month - 1
    && day === inst.selectedDay)
        return;

    if (d.getMonth() !== month - 1) {
        d = new Date(year, month, 1);
        d.setDate(0);
    }

    $ctl.datetimepicker("setDate", d);
}

DSOControls.DateTimeBox.setMonth = function($ctl, year, month) {
    if ($ctl.attr("monthDayValue") == undefined)
        return;

    var day = parseInt($ctl.attr("monthDayValue"));
    var d = new Date(year, month - 1, day);
    $ctl.datetimepicker("setDate", d);
    if (d.getMonth() !== month - 1) {
        d = new Date(year, month, 1);
        d.setDate(0);
        $ctl.datetimepicker("setDate", d);
    }
}

//Autocomplete-------------------------------------------------------------------------
DSOControls.Autocomplete.OnSelect = function(event, ui) {
    var autoPostBack = eval($(this).attr("autoPostBack"));
    var isDropDown = eval($(this).attr("isDropDown"));
    var $txtValue = $($(this).attr("textValueID"));
    var valor = (ui.item.id !== undefined ? ui.item.id : ui.item.value);
    var change = $txtValue.val() != valor;
    var changeTxt = false;

    if ($(this).attr("value") !== ui.item.value) {
        changeTxt = true;
    }
    $(this).attr("value", ui.item.value);
    $txtValue.val(valor);
    this.texto = ui.item.value;

    if (autoPostBack && change) {
        $txtValue.change();
    }
    if (changeTxt) {
        $(this).change();
    }
}
DSOControls.Autocomplete.OnBlur = function() {
    var isDropDown = eval($(this).attr("isDropDown"));
    var $txtValue = $($(this).attr("textValueID"));
    if ($(this).val() !== this.texto) {
        if (isDropDown)
            $(this).val(this.texto);
        else
            $txtValue.val("");
    }
}

DSOControls.Autocomplete.Init = function() {
    $('.DSOAutocomplete').each(function() {
        var options = {};
        if ($(this).attr('isDisabled') != undefined)
            options.disabled = eval($(this).attr('isDisabled'));
        if ($(this).attr('delay') != undefined)
            options.delay = eval($(this).attr('delay'));
        if ($(this).attr('minLength') != undefined)
            options.minLength = eval($(this).attr('minLength'));
        if ($(this).attr('source') != undefined) {
            if ($(this).attr('fnSearch') != undefined)
                options.source = DSOControls.LoadFunction($(this).attr("fnSearch"));
            else
                options.source = DSOControls.Autocomplete.Source;
        }
        else if ($(this).attr('dataSource') != undefined)
            options.source = JSON.parse($(this).attr('dataSource'));

        if ($(this).attr('create') !== undefined)
            options.create = DSOControls.LoadFunction($(this).attr("create"));
        if ($(this).attr('search') !== undefined)
            options.search = DSOControls.LoadFunction($(this).attr("search"));
        if ($(this).attr('open') !== undefined)
            options.open = DSOControls.LoadFunction($(this).attr("open"));
        if ($(this).attr('focus') !== undefined)
            options.focus = DSOControls.LoadFunction($(this).attr("focus"));
        if ($(this).attr('seleccion') != undefined)
            options.select = DSOControls.LoadFunction($(this).attr("seleccion"));
        if ($(this).attr('close') != undefined)
            options.close = DSOControls.LoadFunction($(this).attr("close"));
        if ($(this).attr('change') != undefined)
            options.change = DSOControls.LoadFunction($(this).attr("change"));

        if (eval($(this).attr("isDropDown"))) {
            options.autoFocus = true;
        }
        this.texto = $(this).val();
        this.cache = {};
        this.lastXhr = null;
        $(this).autocomplete(options);
        $(this).bind("autocompleteselect", DSOControls.Autocomplete.OnSelect);
        $(this).bind("blur", DSOControls.Autocomplete.OnBlur);
        $(this).click(function() { $(this).autocomplete("search"); });
    })
}

DSOControls.Autocomplete.Source = function(request, response) {
    var term = request.term;
    if (term in this.element[0].cache) {
        response(this.element[0].cache[term]);
        return;
    }
    var param = { term: term };
    var options = {
        type: "POST",
        url: $(this.element).attr("source"),
        data: JSON.stringify(param),
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        async: true,
        context: this.element[0],
        success: function(data, status, xhr) {
            var results = (typeof data.d) == 'string' ?
                        eval('(' + data.d + ')') :
                        data.d;

            this.cache[term] = results;
            if (xhr === this.lastXhr) {
                response(results);
            }
        },
        error: DSOControls.ErrorAjax
    };
    this.element[0].lastXhr = $.ajax(options);
}

//Expandable-------------------------------------------------------------------------
DSOControls.Expandable.InitElement = function() {
    var options = {};
    if ($(this).attr('startopen') != undefined)
        options.startopen = eval($(this).attr('startopen'));
    if ($(this).attr('titulo') != undefined)
        options.title = $(this).attr('titulo');
//    if ($(this).attr('tooltip') != undefined)
//        options.tooltip = $(this).attr('tooltip');
    if ($(this).attr('uiIconClosed') != undefined)
        options.uiIconClosed = $(this).attr('uiIconClosed');
    if ($(this).attr('uiIconOpen') != undefined)
        options.uiIconOpen = $(this).attr('uiIconOpen');
    if ($(this).attr('duration') != undefined)
        options.duration = eval($(this).attr("duration"));
    if ($(this).attr('easing') != undefined)
        options.easing = $(this).attr('easing');
    if ($(this).attr('open') != undefined)
        options.open = DSOControls.LoadFunction($(this).attr("open"));
    if ($(this).attr('close') != undefined)
        options.close = DSOControls.LoadFunction($(this).attr("close"));
    if ($(this).attr('extraIcon') != undefined)
        options.extraIcon = $(this).attr('extraIcon');
    if ($(this).attr('textOptions') != undefined)
        options.textOptions = $(this).attr('textOptions');

    $(this).expandable(options);
}

DSOControls.Expandable.Init = function() {
    $('.DSOExpandable').each(DSOControls.Expandable.InitElement);
}

//RadioButtonList-------------------------------------------------------------------------

DSOControls.RadioButtonList.Fill = function(response, textStatus, jqXHR) {
    var opciones = eval('(' + response.d + ')');
    var maxLength = opciones.length;
    var columns = $(this).attr("columns");
    var maxRow;
    var i;
    var j;
    var k;
    var value = $(this).find("input[type='radio']:checked").first().val();
    var name = $(this).find("input[type='radio']").first().attr("name");
    var noSelect;

    if ($(this).attr("selectItemText") != undefined && $(this).attr("selectItemText") != "") {
        noSelect = { value: $(this).attr("selectItemValue"), text: $(this).attr("selectItemText") };
        if (maxLength > 0) {
            opciones.splice(0, 0, noSelect);
        }
        else {
            opciones = [];
            opciones.push(noSelect);
        }
        maxLength = maxLength + 1;
    }

    maxRow = this.rows.length;
    for (i = 0; i < maxRow; i++) {
        this.deleteRow(0);
    }

    maxRow = Math.ceil(maxLength / columns);
    for (i = 0; i < maxRow; i++) {
        this.insertRow(0);
        for (j = 0; j < columns; j++) {
            this.rows[0].insertCell(0);
        }
    }

    i = 0;
    j = 0;
    this.itemsCount = maxLength;
    var radio;
    var label;

    for (k = 0; k < maxLength; k++) {
        //        radio = document.createElement("input");
        //        radio.id = this.id + "_" + k;
        //        radio.type = "radio";
        //        radio.name = name;
        //        radio.value = opciones[k].value;
        radio = $("<input type='radio' id = '" + (this.id + "_" + k) + "' name='" + name + "'/>").val(opciones[k].value);

        //Revisar el valor seleccionado anteriormente
        //        if (radio.value == value) $(radio).attr("checked", "checked");
        if (radio.val() == value) radio.attr("checked", "checked");

        //Agregar funcionalidad de javascript a los nuevos elementos
        $(this.attributes).each(function() {
            if (this.name.indexOf("RadioItem-") == 0) {
                radio.attr(this.name.substring(10), this.value);
            }
        });
        radio.change(DSOControls.RadioButtonList.Change);

        label = document.createElement("label");
        $(label).attr("for", radio.attr("id"));
        $(label).text(opciones[k].text);

        $(this.rows[i].cells[j]).append(radio);
        $(this.rows[i].cells[j]).append(label);

        if ($(this).attr("direction") == "Horizontal") {
            j = j + 1;
            if (j == columns) {
                i = i + 1;
                j = 0;
            }
        } else {
            i = i + 1;
            if (i == maxRow) {
                i = 0;
                j = j + 1;
            }
        }
    }
    $(this).find("input[type='radio']").change(DSOControls.RadioButtonList.Change);
    $(this).attr("loaded", true);
}

DSOControls.RadioButtonList.Change = function() {
    $($(this).closest(".DSORadioButtonList").attr("TextValue")).val($(this).val());
}

//Button-------------------------------------------------------------------------
DSOControls.Button.preventDefault = function(event) {
    event.returnValue = false;
}

//CheckBoxList-------------------------------------------------------------------------

DSOControls.CheckBoxList.Check = function(chklst, check) {
    $(chklst).find("input:checkbox").each(function() {
        this.checked = check;
    });
}

DSOControls.CheckBoxList.GetValues = function(chklst, retArray) {
    var values = [];
    $(chklst).find("input:checkbox:checked").each(function() {
        values.push($(this).closest('span').attr('data-value'));
    });
    if (retArray != null && retArray) {
        return values;
    }
    else {
        var separator = $(chklst).attr("separator");
        return values.join(separator);
    }
}

DSOControls.CheckBoxList.GetTextValues = function() {
    var values = [];
    $(this).find("input:checkbox:checked").each(function() {
        values.push($(this).next('label').text());
    });
    var separator = $(this).attr("separator");
    separator = " " + separator + " ";
    return values.join(separator);
}

DSOControls.CheckBoxList.SetTitle = function() {
    var $chklst = $(this).find(".DSOCheckBoxList");
    var title = DSOControls.CheckBoxList.GetTextValues.call($chklst);
    DSOControls.CheckBoxList.UpdateTitle.call(this, title);
}

DSOControls.CheckBoxList.UpdateTitle = function(title) {
    try {
        var textOptions = $(this).attr('textOptions')
        var currentValues = JSON.parse($(textOptions).val());
        currentValues = $.extend(currentValues, { title: title, tooltip: title });
        $(textOptions).val(JSON.stringify(currentValues));
    } catch (e) { }

    DSOControls.Expandable.InitElement.call(this);
}

DSOControls.CheckBoxList.CleanTitle = function() {
    DSOControls.CheckBoxList.UpdateTitle.call(this, " ");
}

DSOControls.CheckBoxList.Change = function() {
    var chklst = $(this).closest(".DSOCheckBoxList");
    var values = DSOControls.CheckBoxList.GetValues(chklst, false);
    chklst.prev(".DSOCheckBoxListVal").val(values);
}

DSOControls.CheckBoxList.Blur = function() {
    var values = DSOControls.CheckBoxList.GetValues(this, false);
    $(this).prev(".DSOCheckBoxListVal").val(values);
}

DSOControls.CheckBoxList.Init = function() {
    $(".DSOCheckBoxList").each(function() {
        $(this).blur(DSOControls.CheckBoxList.Blur);
        $(this).find("input:checkbox").change(DSOControls.CheckBoxList.Change);

        if ($(this).attr("wrapperType") == "DSOExpandable") {
            var $exp = $(this).closest(".DSOExpandable");
            var $textOptions = $($exp.attr("textOptions"));
            var currentValues;
            try {
                currentValues = JSON.parse($textOptions.val());
                if (currentValues && currentValues.startopen) {
                    currentValues.title = " ";
                }
                else {
                    currentValues.title = DSOControls.CheckBoxList.GetTextValues.call(this);
                }
                $textOptions.val(JSON.stringify(currentValues));
            } catch (e) { }
        }
    });
}

DSOControls.CheckBoxList.Fill = function(data, textStatus, jqXHR) {
    var options = (typeof data.d) == 'string' ?
                        eval('(' + data.d + ')') :
                        data.d;
    var maxLength = options == null ? 0 : options.length;
    var columns = $(this).attr("columns");
    var maxRow;
    var maxRowCol;
    var i;
    var j;
    var k;
    var values = [];

    values = DSOControls.CheckBoxList.GetValues(this, true);

    maxRow = this.rows.length;
    for (i = 0; i < maxRow; i++) {
        this.deleteRow(0);
    }

    maxRow = Math.ceil(maxLength / columns);
    maxRowCol = maxLength - (maxRow - 1) * columns;
    for (i = 0; i < maxRow; i++) {
        this.insertRow(0);
        for (j = 0; j < columns; j++) {
            this.rows[0].insertCell(0);
        }
    }

    i = 0;
    j = 0;
    var span;
    var checkbox;
    var label;

    for (k = 0; k < maxLength; k++) {
        span = document.createElement("span");
        $(span).attr("data-value", options[k].value);

        checkbox = document.createElement("input");
        checkbox.id = this.id + "_" + k;
        checkbox.type = "checkbox";
        checkbox.name = checkbox.id.replace("_", "$");

        if ($.inArray(options[k].value.toString(), values) > -1) {
            checkbox.checked = true;
        }

        $(this.attributes).each(function() {
            if (this.name.indexOf("CheckItem-") == 0) {
                $(checkbox).attr(this.name.substring(10), this.value);
            }
        });
        $(checkbox).change(DSOControls.CheckBoxList.Change);

        label = document.createElement("label");
        $(label).attr("for", checkbox.id);
        label.innerHTML = options[k].text;

        span.appendChild(checkbox);
        span.appendChild(label);
        this.rows[i].cells[j].appendChild(span);

        if ($(this).attr("direction") == "Horizontal") {
            j = j + 1;
            if (j == columns) {
                i = i + 1;
                j = 0;
            }
        } else {
            i = i + 1;
            if (i == maxRow) {
                i = 0;
                j = j + 1;
                if (j >= maxRowCol) {
                    maxRow = maxRow - 1;
                    maxRowCol = columns;
                }
            }
        }
    }
    $(this).blur();
}

//Flags------------------------------------------------------------------------------
DSOControls.Flags.GetValue = function() {
    var value = 0;
    var itmValue;
    $(this).find("input:checkbox:checked").each(function() {
        //itmValue = Math.pow(2, parseInt($(this).closest('span').attr('data-value')));
        itmValue = parseInt($(this).closest('span').attr('data-value'));
        value += itmValue;
    });
    return value;
}

DSOControls.Flags.SetTitle = function() {
    var $chklst = $(this).find(".DSOFlags");
    var title = DSOControls.CheckBoxList.GetTextValues.call($chklst);
    DSOControls.CheckBoxList.UpdateTitle.call(this, title);
}

DSOControls.Flags.Change = function() {
    var $chklst = $(this).closest(".DSOFlags");
    var value = DSOControls.Flags.GetValue.call($chklst);
    $chklst.prev(".DSOFlagsVal").val(value);
}

DSOControls.Flags.Blur = function() {
    var value = DSOControls.Flags.GetValue.call(this);
    $(this).prev(".DSOFlagsVal").val(value);
}

DSOControls.Flags.Init = function() {
    $(".DSOFlags").each(function() {
        $(this).blur(DSOControls.Flags.Blur);
        $(this).find("input:checkbox").change(DSOControls.Flags.Change);

        if ($(this).attr("wrapperType") == "DSOExpandable") {
            var $exp = $(this).closest(".DSOExpandable");
            var $textOptions = $($exp.attr("textOptions"));
            var currentValues;
            try {
                currentValues = JSON.parse($textOptions.val());
                if (currentValues && currentValues.startopen) {
                    currentValues.title = " ";
                }
                else {
                    currentValues.title = DSOControls.CheckBoxList.GetTextValues.call(this);
                }
                $textOptions.val(JSON.stringify(currentValues));
            } catch (e) { }
        }
    });
}

//NumberEdit-------------------------------------------------------------------------
DSOControls.NumberEdit.Init = function() {
    $(".DSONumberEdit").each(function() {
        var options = {};
        if ($(this).attr("aNeg") !== undefined) { options.aNeg = $(this).attr("aNeg"); }
        if ($(this).attr("aSep") !== undefined) { options.aSep = $(this).attr("aSep"); }
        if ($(this).attr("aDec") !== undefined) { options.aDec = $(this).attr("aDec"); }
        if ($(this).attr("aSign") !== undefined) { options.aSign = $(this).attr("aSign"); }
        if ($(this).attr("pSign") !== undefined) { options.pSign = $(this).attr("pSign"); }
        if ($(this).attr("mNum") !== undefined) { options.mNum = parseInt($(this).attr("mNum")); }
        if ($(this).attr("mDec") !== undefined) { options.mDec = parseInt($(this).attr("mDec")); }
        if ($(this).attr("dGroup") !== undefined) { options.dGroup = parseInt($(this).attr("dGroup")); }
        if ($(this).attr("mRound") !== undefined) { options.mRound = $(this).attr("mRound"); }
        if ($(this).attr("aPad") !== undefined) { options.aPad = eval($(this).attr("aPad")); }
        $(this).autoNumeric(options);
    });
}

//Window-------------------------------------------------------------------------
DSOControls.Window.Init = function() {
    var options = {};
    if ($(this).attr('resizeable') != undefined)
        options.resizeable = eval($(this).attr('resizeable'));
    if ($(this).attr('minimizeButton') != undefined)
        options.minimizeButton = eval($(this).attr('minimizeButton'));
    if ($(this).attr('maximizeButton') != undefined)
        options.maximizeButton = eval($(this).attr('maximizeButton'));
    if ($(this).attr('closeButton') != undefined)
        options.closeButton = eval($(this).attr('closeButton'));
    if ($(this).attr('statusBar') != undefined)
        options.statusBar = eval($(this).attr('statusBar'));
    if ($(this).attr('modal') != undefined)
        options.modal = eval($(this).attr('modal'));
    if ($(this).attr('display') != undefined)
        options.display = eval($(this).attr('display'));

    if ($(this).attr('type') != undefined)
        options.type = $(this).attr('type');
    if ($(this).attr('state') != undefined)
        options.state = $(this).attr('state');
    if ($(this).attr('title') != undefined)
        options.title = $(this).attr('title');
    if ($(this).attr('posy') != undefined)
        options.posy = eval($(this).attr('posy'));
    if ($(this).attr('posx') != undefined)
        options.posx = eval($(this).attr('posx'));
    if ($(this).attr('width') != undefined)
        options.width = eval($(this).attr('width'));
    if ($(this).attr('height') != undefined)
        options.height = eval($(this).attr('height'));
    if ($(this).attr('onDragBegin') != undefined)
        options.onDragBegin = DSOControls.LoadFunction($(this).attr("onDragBegin"));
    if ($(this).attr('onDragEnd') != undefined)
        options.onDragEnd = DSOControls.LoadFunction($(this).attr("onDragEnd"));
    if ($(this).attr('onResizeBegin') != undefined)
        options.onResizeBegin = DSOControls.LoadFunction($(this).attr("onResizeBegin"));
    if ($(this).attr('onResizeEnd') != undefined)
        options.onResizeEnd = DSOControls.LoadFunction($(this).attr("onResizeEnd"));
    if ($(this).attr('onAjaxContentLoaded') != undefined)
        options.onAjaxContentLoaded = DSOControls.LoadFunction($(this).attr("onAjaxContentLoaded"));
    if ($(this).attr('onWindowClose') != undefined)
        options.onWindowClose = DSOControls.LoadFunction($(this).attr("onWindowClose"));
    if ($(this).attr('textOptions') != undefined)
        options.textOptions = $(this).attr('textOptions');

    $(this).newWindow(options);
    if ($(this).attr('src') != undefined && ($(this).attr('type') == undefined || $(this).attr('type') == "normal"))
        $(this).updateWindowContentWithAjax($(this).attr('src'));
}


DSOControls.Window.InitOnReady = function() {
    $('.DSOWindow[initOnReady=true]').each(DSOControls.Window.Init);
}

//Tabs-------------------------------------------------------------------------------------------
DSOControls.Tabs.UpdateState = function(update, $tabs) {
    var $txtState = $($tabs.attr("txtStateID"));

    var jsonValue = $txtState.val()
    var prevState = jsonValue !== undefined && jsonValue !== "" ? JSON.parse(jsonValue) : {};
    var newState = $.extend({ autoPostBack: false, order: [], selectedIndex: 0 }, prevState, update);

    $txtState.val(JSON.stringify(newState));
    if (newState.autoPostBack && jsonValue !== txtState.val()) {
        $txtState.change();
    }
}

DSOControls.Tabs.SortStop = function(event, ui) {
    var $tabs = $(ui.item).closest(".DSOTabsContainer");
    var data = $tabs.children(".DSOTabHeaders:first").children(".DSOTabHeader").map(function() { return parseInt($(this).attr("itemIndex")); }).get();
    var order = { order: data };

    DSOControls.Tabs.UpdateState(order, $tabs);
}

DSOControls.Tabs.Select = function(event, ui) {
    var $tabs = $(ui.panel).closest(".DSOTabsContainer");
    var $li = $(ui.tab).closest(".DSOTabHeader");
    var selected = { selectedIndex: parseInt($li.attr("itemIndex")) };

    DSOControls.Tabs.UpdateState(selected, $tabs);
}

DSOControls.Tabs.Init = function() {
    $(".DSOTabsContainer").each(function() {
        var options = {
            ajaxOptions: { error: DSOControls.ErrorAjax },
            selected: parseInt($(this).attr("selectedIndex"))
        }

        if ($(this).attr("onTabsCreate") !== undefined) {
            options.create = DSOControls.LoadFunction($(this).attr("onTabsCreate"));
        }
        if ($(this).attr("onTabsSelect") !== undefined) {
            options.select = DSOControls.LoadFunction($(this).attr("onTabsSelect"));
        }
        if ($(this).attr("onTabsLoad") !== undefined) {
            options.load = DSOControls.LoadFunction($(this).attr("onTabsLoad"));
        }
        if ($(this).attr("onTabsShow") !== undefined) {
            options.show = DSOControls.LoadFunction($(this).attr("onTabsShow"));
        }
        if ($(this).attr("onTabsAdd") !== undefined) {
            options.add = DSOControls.LoadFunction($(this).attr("onTabsAdd"));
        }
        if ($(this).attr("onTabsRemove") !== undefined) {
            options.remove = DSOControls.LoadFunction($(this).attr("onTabsRemove"));
        }
        if ($(this).attr("onTabsEnable") !== undefined) {
            options.enable = DSOControls.LoadFunction($(this).attr("onTabsEnable"));
        }
        if ($(this).attr("onTabsDisable") !== undefined) {
            options.disable = DSOControls.LoadFunction($(this).attr("onTabsDisable"));
        }

        var isSortable = eval($(this).attr("isSortable"));
        if (isSortable) {
            $(this).tabs(options).find(".ui-tabs-nav").sortable({
                axis: "x",
                stop: DSOControls.Tabs.SortStop
            });
        }
        else {
            $(this).tabs(options);
        }

        $(this).bind("tabsselect", DSOControls.Tabs.Select);
    });
}

//Grid-------------------------------------------------------------------------------------------
DSOControls.Grid.Init = {}

DSOControls.Grid.GetRequest = function(aoData) {
    var request = {};
    var maxIdx = aoData.length;
    for (var idx = 0; idx < maxIdx; idx++) {
        var param = aoData[idx];
        if (param.name === "sEcho")
            request.sEcho = parseInt(param.value);
        if (param.name === "iColumns")
            request.iColumns = param.value;
        if (param.name === "sColumns")
            request.sColumns = param.value;
        if (param.name === "iDisplayStart")
            request.iDisplayStart = param.value;
        if (param.name === "iDisplayLength")
            request.iDisplayLength = param.value;
        if (param.name === "sSearch")
            request.sSearchGlobal = param.value;
        if (param.name === "bRegex")
            request.bEscapeRegexGlobal = param.value;
        if (param.name.indexOf("sSearch_") == 0) {
            if (request.sSearch === undefined)
                request.sSearch = [];
            request.sSearch.push(param.value);
        }
        if (param.name.indexOf("bRegex_") == 0) {
            if (request.bEscapeRegex === undefined)
                request.bEscapeRegex = [];
            request.bEscapeRegex.push(param.value);
        }
        if (param.name.indexOf("bSearchable_") == 0) {
            if (request.bSearchable === undefined)
                request.bSearchable = [];
            request.bSearchable.push(param.value);
        }
        if (param.name === "iSortingCols")
            request.iSortingCols = param.value;
        if (param.name.indexOf("iSortCol_") == 0) {
            if (request.iSortCol === undefined)
                request.iSortCol = [];
            request.iSortCol.push(param.value);
        }
        if (param.name.indexOf("sSortDir_") == 0) {
            if (request.sSortDir === undefined)
                request.sSortDir = [];
            request.sSortDir.push(param.value);
        }
        if (param.name.indexOf("bSortable_") == 0) {
            if (request.bSortable === undefined)
                request.bSortable = [];
            request.bSortable.push(param.value);
        }
    }
    return request;
}

//Button--------------------------------------------
DSOControls.Button.Init = function() {
    $(".button").button();
    $(".buttonPlay").button({ icons: { primary: "custom-icon-play"} });
    $(".buttonPlayImg").button({ icons: { primary: "custom-icon-play" }, text: false });
    $(".buttonAdd").button({ icons: { primary: "custom-icon-add"} });
    $(".buttonAddImg").button({ icons: { primary: "custom-icon-add" }, text: false });
    $(".buttonSave").button({ icons: { primary: "custom-icon-save"} });
    $(".buttonEdit").button({ icons: { primary: "custom-icon-edit"} });
    $(".buttonEditImg").button({ icons: { primary: "custom-icon-edit" }, text: false });
    $(".buttonCancel").button({ icons: { primary: "custom-icon-cancel"} });
    $(".buttonDelete").button({ icons: { primary: "custom-icon-delete"} });
    $(".buttonSearch").button({ icons: { primary: "custom-icon-search"} });
    $(".buttonSearchImg").button({ icons: { primary: "custom-icon-search" }, text: false });
    $(".buttonOK").button({ icons: { primary: "custom-icon-ok"} });
    $(".buttonBack").button({ icons: { primary: "custom-icon-back"} });
    $(".buttonXLS").button({ icons: { primary: "custom-icon-xls"} });

    $(".button").css("display", "");
    $(".buttonPlay").css("display", "");
    $(".buttonPlayImg").css("display", "");    
    $(".buttonAdd").css("display", "");
    $(".buttonAddImg").css("display", "");
    $(".buttonSave").css("display", "");
    $(".buttonEdit").css("display", "");
    $(".buttonEditImg").css("display", "");
    $(".buttonCancel").css("display", "");
    $(".buttonDelete").css("display", "");
    $(".buttonSearch").css("display", "");
    $(".buttonSearchImg").css("display", "");
    $(".buttonOK").css("display", "");
    $(".buttonBack").css("display", "");
    $(".buttonXLS").css("display", "");
}

//Inicializacion General-------------------------------------------------------------------------
$(document).ready(function() {
    $("textarea[maxlength]").keyup(DSOControls.TextBox.CheckMaxLength);
    $(".DSODropDownList").change(DSOControls.DropDown.Change);
    $(".DSOListBox[TextValue][Separator]").change(DSOControls.ListBox.Change);
    $(".DSORadioButtonList[TextValue] input[type='radio']").change(DSOControls.RadioButtonList.Change);
    DSOControls.DateTimeBox.Init();
    DSOControls.CheckBoxList.Init();
    DSOControls.Flags.Init();
    DSOControls.NumberEdit.Init();
    DSOControls.Expandable.Init();
    DSOControls.Window.InitOnReady();
    DSOControls.Autocomplete.Init();
    DSOControls.Tabs.Init();
    DSOControls.Button.Init();
});