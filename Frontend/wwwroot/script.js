window.getToken = () => {
    return token = localStorage.getItem("AUTH_TOKEN");
}

window.storeToken = (token) => {
    localStorage.setItem("AUTH_TOKEN", token);
}

window.deleteToken = () => {
    localStorage.removeItem("AUTH_TOKEN");
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

window.setInnerHtml = (elem, text) => {
    elem.innerHTML = text;
}

window.initializeCarousel = () => {
    $('#carousel').carousel({interval: 2000});
    $('.carousel-control-prev').click ( 
            () => $('#carousel').carousel('prev') );
    $('.carousel-control-next').click ( 
            () => $('#carousel').carousel('next') );
}

let handler;

window.addKeyListener = (dotNetHelper) => {
    handler = (event) => {
        dotNetHelper.invokeMethodAsync("CloseModalKeyDown", event.key);
    }
    document.addEventListener("keydown", handler);
};

window.removeKeyListener = () => {
    document.removeEventListener("keydown", handler);
}