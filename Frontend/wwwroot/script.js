window.getToken = () => {
    return token = localStorage.getItem("AUTH_TOKEN");
}

window.storeToken = (token) => {
    localStorage.setItem("AUTH_TOKEN", token);
}