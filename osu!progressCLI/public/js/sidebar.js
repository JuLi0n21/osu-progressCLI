document.addEventListener("DOMContentLoaded", function () {
    const sidebarLinks = document.querySelectorAll(".nav-link");
    const sections = document.querySelectorAll("h1, h2");

    const sidebar = document.querySelector(".sidebar"); // Change this line to use querySelector
    const toggleButton = document.getElementById("toggleButton");
    toggleButton.addEventListener("click", function () {
        console.log("click")
      sidebar.classList.toggle("hidebar");
    });
});