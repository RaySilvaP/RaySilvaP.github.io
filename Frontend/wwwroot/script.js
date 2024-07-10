window.getToken = () => {
    return token = localStorage.getItem("AUTH_TOKEN");
}

window.storeToken = (token) => {
    localStorage.setItem("AUTH_TOKEN", token);
}

window.previewImages = (inputElem, parentElem) => {
    for(const child in parentElem.children){
        URL.revokeObjectURL(child.src);
    }
    parentElem.innerHTML = "";

    for (let i = 0; i < inputElem.files.length; i++) {
        const url = URL.createObjectURL(inputElem.files[i]);
        const imgElem = document.createElement("img");
        imgElem.src = url;
        parentElem.appendChild(imgElem);
    }
}