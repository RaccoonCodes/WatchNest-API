$(document).on('click', '.pagination-link', function (e) {
    e.preventDefault(); // Prevent the default link behavior

    const url = $(this).data('url'); // Get the URL from the data attribute
    const viewType = $('#table-content').data('viewtype'); // Determine if it's Admin or User
    const dataType = $('#table-content').data('datatype'); // Determine data type

    if (!url || !viewType || !dataType)
        return; // Do nothing if URL or view type is not set

    const tableContentId = '#table-content';
    const paginationContainerId = '#pagination-container';

    const urlMapping = {
        Admin: {
            UserModel: { tableUpdateUrl: '/Admin/PartialUserTable', paginationUpdateUrl: '/Admin/PaginationUpdate' },
            SeriesDTO: { tableUpdateUrl: '/Admin/PartialTableUpdate', paginationUpdateUrl: '/Admin/PaginationUpdate' },

        },
        User: {
            SeriesDTO: { tableUpdateUrl: '/WatchList/PartialTableUpdate', paginationUpdateUrl: '/WatchList/PaginationUpdate' },
        },
    };

    const updateUrls = urlMapping[viewType]?.[dataType];
    if (!updateUrls)
        return; // Do nothing for invalid viewType or dataType

    const { tableUpdateUrl, paginationUpdateUrl } = updateUrls;
    $.ajax({
        url: url,
        type: 'GET',
        headers: { 'X-Requested-With': 'XMLHttpRequest' },
        xhrFields: { withCredentials: true },
        success: function (result) {
            const apiResponse = {
                data: result.data,
                pageIndex: result.pageIndex,
                recordCount: result.recordCount,
                totalPages: result.totalPages,
                links: result.links,
            };

            // Update Partial Table
            updatePartial(tableUpdateUrl, apiResponse.data, tableContentId);

            // Update Pagination
            updatePartial(paginationUpdateUrl, apiResponse, paginationContainerId);
        },
        error: function (status, error) {
            console.error("Pagination error:", status, error);
            alert("An error occurred while loading the page.");
        },
    });

    function updatePartial(url, data, containerId) {
        $.ajax({
            url: url,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            success: function (partialHtml) {
                $(containerId).html(partialHtml);
            },
            error: function (status, error) {
                console.error(`Error updating content for ${containerId}:`, status, error);
                alert(`An error occurred while updating the content for ${containerId}.`);
            },
        });
    }

});
