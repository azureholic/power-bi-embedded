function listReports(report) {
    var reportbuttons = document.getElementById("reportbuttons");

    var reportlist = report.embedReport;

    for (let i = 0; i < reportlist.length; i++) {
        let itemContainer = document.createElement("div")
        let item = document.createElement("button");
        item.innerHTML = reportlist[i].reportName;
        item.classList.add("btn")
        item.classList.add("btn-primary")
        item.classList.add("btn-100")
        item.onclick = function () {
            loadReport(report, i); 
        };


        
        itemContainer.appendChild(item)
        reportbuttons.appendChild(itemContainer);
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
                listReports(response);
                loadReport(response, 0);
            }

            return response;
        }).catch(error => {
            console.error(error);
        });
}