document.addEventListener("DOMContentLoaded", function () {
    console.log("RMM Review Page Initialized");

    const form = document.querySelector('form');
    if (form) {
        form.addEventListener('submit', function (e) {
            const rating = document.querySelector('select[name="rating"]').value;
            if (!rating) {
                alert("Please select a rating.");
                e.preventDefault();
            }
        });
    }
});
