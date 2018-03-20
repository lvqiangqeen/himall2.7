var LODOP_Printer; //声明为全局变量       

//选择打印机
function getSelectedPrintIndex() {
    if (document.getElementById("Radio2").checked)
        return document.getElementById("PrinterList").value;
    else return -1;
};


function createPrinterList() {
    if (document.getElementById('PrinterList').innerHTML != "") return;
    LODOP_Printer = getLodop();
    var iPrinterCount = LODOP_Printer.GET_PRINTER_COUNT();
    for (var i = 0; i < iPrinterCount; i++) {

        var option = document.createElement('option');
        option.innerHTML = LODOP_Printer.GET_PRINTER_NAME(i);
        option.value = i;
        document.getElementById('PrinterList').appendChild(option);
    };
};

function getPrintsCount() {
    LODOP_Printer = getLodop();
    var iPrinterCount = LODOP_Printer.GET_PRINTER_COUNT();
    return iPrinterCount;
}

function LodopPrint(width, height, fontSize, data, askForSelectPrinter, preview, printRightNow) {
    if (askForSelectPrinter && getPrintsCount>1) {
        $.dialog.confirm('<p>选择如下打印机：<input type="radio" id="Radio1" name="RadioS1" checked />默认打印机<input type="radio" id="Radio2" name="RadioS1" onclick="CreatePrinterList()" />指定打印机:  <select id="PrinterList" size="1"></select></p> ',
         function () {
             LODOP_Printer.SET_PRINTER_INDEX(getSelectedPrintIndex());
             loadpPrintRightnow(width, height, fontSize, data, preview, printRightNow);
          });

    }
    else
        loadpPrintRightnow(width, height, fontSize, data, preview, printRightNow);
}

function loadpPrintRightnow(width, height, fontSize, data, preview, printRightNow) {
    LODOP_Printer = getLodop();
    LODOP_Printer.SET_PRINT_PAGESIZE(1, width, height, "");
    LODOP_Printer.SET_PRINT_STYLE("FontSize", fontSize);        $.each(data, function (i, printItem) {
       // console.log(printItem.Y + '%', printItem.X + '%', printItem.Width + '%', printItem.Height + '%', printItem.Value);
        LODOP_Printer.ADD_PRINT_TEXT(parseInt(printItem.Y) / 100 + '%', parseInt(printItem.X) / 100 + '%', parseInt(printItem.Width) / 100 + '%', parseInt(printItem.Height) / 100 + '%', printItem.Value);
    });
    if (preview)
        LODOP_Printer.PREVIEW();

    printRightNow = printRightNow == null ? true : printRightNow;
    if (printRightNow)
        LODOP_Printer.PRINT();
}

