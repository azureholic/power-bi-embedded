const msal = require('msal');

const config = {
    auth: {
        clientId: "3b52108a-5b62-4b0d-914f-eb6da7913f1c",
        authority: "https://login.microsoftonline.com/1d3fe57e-e849-4872-abee-c41ab915ef28/",
        redirectUri: "http://localhost:54620/"
    }
}
const pca = new PublicClientApplication(config);

// You can also provide the authority as part of the request object
const request = {
    scopes: ["openid"],
    authority: "https://yourApp.b2clogin.com/yourApp.onmicrosoft.com/your_policy"
}
pca.loginRedirect(request);