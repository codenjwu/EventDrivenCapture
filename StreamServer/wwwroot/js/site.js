"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/StreamingHub").build();

connection.on("ReceiveImage", function (image) {
    document.getElementById("myRoom").src = image;
});

connection.start().then(function () {
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("start").addEventListener("click", function (event) {
    var x = document.getElementById("x").value;
    var y = document.getElementById("y").value;
    var w = document.getElementById("w").value;
    var h = document.getElementById("h").value;
    connection.invoke("SendImage", 0, parseInt(x), parseInt(y), parseInt(w),parseInt(h),false).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});
