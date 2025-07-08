<script>
    document.addEventListener("DOMContentLoaded", function () {
        // Get all nav links
        const navLinks = document.querySelectorAll(".nav-link");
    const sections = document.querySelectorAll("section");

    // Function to hide all sections except the one clicked
    function showSection(targetId) {
        sections.forEach(section => {
            if (section.id === targetId) {
                section.style.display = "block";  // Show selected section
            } else {
                section.style.display = "none";  // Hide others
            }
        });
        }

    // Initially, only show the home section
    showSection("homeSection");

        // Add click event to each nav item
        navLinks.forEach(link => {
        link.addEventListener("click", function (e) {
            e.preventDefault(); // Prevent default jump
            const targetId = this.getAttribute("href").substring(1); // Get section ID

            // Show the clicked section
            showSection(targetId);
        });
        });
    });
</script>
