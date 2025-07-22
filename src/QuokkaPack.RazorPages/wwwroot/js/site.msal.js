async function submitCategory() {
    const name = document.getElementById('categoryName').value.trim();
    const isDefault = document.getElementById('isDefault').checked;

    if (!name) {
        alert("Category name is required.");
        return;
    }

    let token;
    try {
        token = await signInAndGetToken();
        console.log("Access token:", token);
    } catch (err) {
        alert("Failed to acquire token: " + err.message);
        return;
    }

    const payload = { name, isDefault };

    const response = await fetch('/api/categories', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(payload)
    });

    if (response.ok) {
        location.reload();
    } else {
        const errorText = await response.text();
        alert("Failed to add category: " + errorText);
    }
}

document.addEventListener("DOMContentLoaded", function () {
    const msalConfig = {
        auth: {
            clientId: '3a4b7f17-a1a7-4504-a388-c8def0b39115',
            authority: 'https://login.microsoftonline.com/c1532d29-6f77-4b68-a33c-b769515dfd69',
            redirectUri: window.location.origin
        }
    };

    const tokenRequest = {
        scopes: ["api://87ecda94-4353-47fc-bbad-cb588ba2199c/access_as_user"]
    };

    const msalInstance = new msal.PublicClientApplication(msalConfig);

    window.signInAndGetToken = async function () {
        try {
            let account = msalInstance.getActiveAccount();
            if (!account) {
                const accounts = msalInstance.getAllAccounts();
                if (accounts.length === 0) {
                    const loginResponse = await msalInstance.loginPopup(tokenRequest);
                    account = loginResponse.account;
                } else {
                    account = accounts[0];
                }
                msalInstance.setActiveAccount(account);
            }

            const tokenResponse = await msalInstance.acquireTokenSilent({
                ...tokenRequest,
                account: msalInstance.getActiveAccount()
            });

            return tokenResponse.accessToken;
        } catch (err) {
            console.warn("Silent token acquisition failed, trying popup...", err);
            const tokenResponse = await msalInstance.acquireTokenPopup(tokenRequest);
            return tokenResponse.accessToken;
        }
    };

    document.getElementById("submitCategoryBtn").addEventListener("click", submitCategory);
    document.getElementById("addCategoryBtn").addEventListener("click", function () {
        const modal = new bootstrap.Modal(document.getElementById("addCategoryModal"));
        modal.show();
    });
});
