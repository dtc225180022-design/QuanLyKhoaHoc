// Global utils
const trangThaiLabel = {
    'MoChuaHoc': '<span class="badge bg-success">Mở đăng ký</span>',
    'DangHoc': '<span class="badge bg-primary">Đang học</span>',
    'DaKetThuc': '<span class="badge bg-secondary">Đã kết thúc</span>',
    'HuyBo': '<span class="badge bg-danger">Hủy bỏ</span>',
    'ChoDuyet': '<span class="badge bg-warning text-dark">Chờ duyệt</span>',
    'DaDuyet': '<span class="badge bg-info">Đã duyệt</span>',
    'HoanThanh': '<span class="badge bg-success">Hoàn thành</span>',
};

function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN').format(amount) + '₫';
}

function formatDate(dateStr) {
    if (!dateStr) return '—';
    return new Date(dateStr).toLocaleDateString('vi-VN');
}

// Route mapping cho SPA-style navigation
document.addEventListener('DOMContentLoaded', function() {
    const path = window.location.pathname;
    // highlight active nav
    document.querySelectorAll('.navbar-nav .nav-link').forEach(link => {
        if (link.getAttribute('href') === path) link.classList.add('active');
    });
});
