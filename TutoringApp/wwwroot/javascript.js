function DownloadFile(fileBytes, fileNameOverride) {
    var fileName = fileNameOverride || "inscricoes.xlsx";
    var blob = new Blob([fileBytes], { type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" });
    if (navigator.msSaveOrOpenBlob) {
        navigator.msSaveBlob(blob, fileName);
    } else {
        var link = document.createElement("a");
        link.href = URL.createObjectURL(blob);
        link.download = fileName;
        link.click();
    }
}