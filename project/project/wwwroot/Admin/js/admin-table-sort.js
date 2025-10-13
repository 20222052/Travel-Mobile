// Admin Table Sort & Search Helper
$(document).ready(function() {
    // Highlight current sort column
    $('th a[asp-route-sortOrder]').each(function() {
        var currentSort = $(this).closest('table').data('current-sort');
        if (currentSort && $(this).attr('href').includes(currentSort)) {
            $(this).addClass('text-primary font-weight-bold');
        }
    });

    // Auto-submit search on Enter key
    $('form[asp-action="Index"] input[name="searchString"]').on('keypress', function(e) {
        if (e.which === 13) {
            $(this).closest('form').submit();
        }
    });

    // Confirm delete
    $('.btn-delete').on('click', function(e) {
        if (!confirm('Bạn có chắc muốn xóa mục này?')) {
            e.preventDefault();
        }
    });
});
