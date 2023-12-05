// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


// Menu for options medias
function toggleMenuOption(event) {
    let button = event.currentTarget;
    // Le menu est le prochain élément frère du bouton dans le DOM
    let navigation = button.nextElementSibling;

    if (navigation.style.display === 'block') {
        navigation.style.display = 'none';
    } else {
        navigation.style.display = 'block';
    }
}

document.addEventListener('DOMContentLoaded', function () {

    let blocMediasWithStreamingLink = document.getElementById('mediasWithStreamingLinkContainer');
    let blocMediasWaiting = document.getElementById('mediasWaitingContainer');

    if (blocMediasWithStreamingLink && blocMediasWaiting) {
        // Pagination Films Disponibles
        function loadMediasWithStreamingLink(pageIndex) {
            fetch(`/Media/GetMediasStreamingLink?pageIndex=${pageIndex}`)
                .then(response => response.text())
                .then(html => {
                    blocMediasWithStreamingLink.innerHTML = html;
                })
                .catch(error => console.error('Error loading medias with DownloadUrl:', error));
        }

        // Pagination Films en Attente
        function loadMediasWaiting(pageIndex) {
            fetch(`/Media/GetMediasWaiting?pageIndex=${pageIndex}`)
                .then(response => response.text())
                .then(html => {
                    blocMediasWaiting.innerHTML = html;
                })
                .catch(error => console.error('Error loading medias waiting:', error));
        }

        loadMediasWithStreamingLink(1); // Charge la première page des films avec DownloadUrl
        loadMediasWaiting(1); // Charge la première page des films en attente

        // Favorite toggle
        document.body.addEventListener('click', function (event) {
            const button = event.target.closest('.toggle-favorite');

            if (button) {
                let mediaId = button.getAttribute('data-media-id');
                let isFavorite = button.getAttribute('data-is-favorite') === 'true';

                // Mettre à jour visuellement le bouton avec les icônes Bootstrap
                button.innerHTML = isFavorite
                    ? '<i class="bi bi-star text-warning"></i>'
                    : '<i class="bi bi-star-fill text-warning"></i>';
                button.setAttribute('data-is-favorite', !isFavorite);

                fetch('/Media/ToggleFavorite', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': antiforgeryToken
                    },
                    body: JSON.stringify({ Media: { Id: mediaId } }) // Envoyer un objet MediaViewModel
                })
                    .then(response => response.json())
                    .then(data => {
                        // Mettre à jour le bouton avec les icônes Bootstrap en fonction de la réponse du serveur
                        button.setAttribute('data-is-favorite', data.isFavorite);
                        button.innerHTML = data.isFavorite
                            ? '<i class="bi bi-star-fill text-warning"></i>'
                            : '<i class="bi bi-star text-warning"></i>';
                    })
                    .catch(error => {
                        console.error('Error:', error);
                        // En cas d'erreur, rétablir l'état initial avec les icônes d'origine
                        button.innerHTML = isFavorite
                            ? '<i class="bi bi-star-fill text-warning"></i>'
                            : '<i class="bi bi-star text-warning"></i>';
                        button.setAttribute('data-is-favorite', isFavorite);
                    });
            }
        });

        // Sort column for the specific table
        document.getElementById('mediasWithStreamingLinkContainer').addEventListener('click', function (event) {
            const header = event.target.closest('th');

            if (header && header.classList.contains('sorted')) {
                const sortOrder = header.dataset.sortOrder === 'asc' ? 'desc' : 'asc';
                const columnIndex = header.cellIndex;

                const rows = Array.from(document.querySelectorAll('#mediasWithStreamingLinkContainer tbody tr'));

                rows.sort((a, b) => {
                    const aValue = getCellValue(a, columnIndex);
                    const bValue = getCellValue(b, columnIndex);

                    // Compare les valeurs en fonction de l'ordre de tri
                    return sortOrder === 'asc' ? aValue.localeCompare(bValue) : bValue.localeCompare(aValue);
                });

                const tbody = document.querySelector('#mediasWithStreamingLinkContainer tbody');
                tbody.innerHTML = '';
                rows.forEach(row => tbody.appendChild(row));

                header.dataset.sortOrder = sortOrder;

                document.querySelectorAll('#mediasWithStreamingLinkContainer th').forEach(otherHeader => {
                    if (otherHeader !== header) {
                        otherHeader.dataset.sortOrder = 'asc';
                    }
                });
            }
        });

        function getCellValue(row, columnIndex) {
            const cell = row.cells[columnIndex];

            // Si la cellule contient un bouton avec la classe 'toggle-favorite', retourne la valeur de l'attribut data-is-favorite
            const button = cell.querySelector('.toggle-favorite');
            if (button) {
                return button.getAttribute('data-is-favorite');
            }

            // Pour les cellules non boutons, retourne le contenu textuel après suppression des espaces
            return cell.textContent.trim();
        }
    };
});

