function listReports(reportlist) {
    var select = document.getElementById("reports");

    for (var i = 0; i < reportlist.length; i++) {
        var item = document.createElement("option")
        item.text = reportlist[i].reportName;
        item.value = JSON.stringify(reportlist[i]);
        select.add(item);
    }
}

function callApi(endpoint, token) {

    const headers = new Headers();
    const bearer = `Bearer ${token}`;

    headers.append("Authorization", bearer);

    const options = {
        method: "GET",
        headers: headers
    };

    logMessage('Calling Web API...');

    fetch(endpoint, options)
        .then(response => response.json())
        .then(response => {

            if (response) {
                logMessage('Reports Found: ' + response.embedReport.length);
                listReports(response.embedReport);
                loadReport(response);
            }

            return response;
        }).catch(error => {
            console.error(error);
        });
}