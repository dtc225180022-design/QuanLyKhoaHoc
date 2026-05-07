// ─── JWT COOKIE SYNC ────────────────────────────────────────────────
// Đồng bộ JWT từ localStorage sang cookie để server-side [Authorize] hoạt động.
// Cookie được set mỗi khi auth.js load (mọi trang).
(function syncJwtCookie() {
    const token = localStorage.getItem('jwt_token');
    if (token) {
        // 8 giờ = 28800 giây (khớp với JWT expiry trong AuthService)
        document.cookie = 'jwt_token=' + token + '; path=/; max-age=28800; SameSite=Lax';
    } else {
        // Xóa cookie nếu không có token
        document.cookie = 'jwt_token=; path=/; max-age=0; SameSite=Lax';
    }
})();

// ─── REDIRECT KHI ĐÃ ĐĂNG NHẬP ─────────────────────────────────────
// Nếu đang ở trang auth mà đã có token → về trang chủ (tránh loop)
(function checkAuthRedirect() {
    const token = localStorage.getItem('jwt_token');
    const path = window.location.pathname.toLowerCase();
    const authPages = ['/auth/dangnhap', '/auth/dangky', '/dang-nhap', '/dang-ky'];
    if (token && authPages.some(p => path === p || path.startsWith(p))) {
        window.location.href = '/';
        return;
    }
})();

// ─── NAVBAR THEO VAI TRÒ ────────────────────────────────────────────
(function initNavbar() {
    const token = localStorage.getItem('jwt_token');
    const hoTen = localStorage.getItem('ho_ten');
    const vaiTro = localStorage.getItem('vai_tro');
    const anhDaiDien = localStorage.getItem('anh_dai_dien');

    const navLoggedIn  = document.getElementById('navLoggedIn');
    const navLoggedOut = document.getElementById('navLoggedOut');
    const navAdmin     = document.getElementById('navAdmin');
    const navGV        = document.getElementById('navGiangVien');
    const navHV        = document.getElementById('navHocVien');
    const navRegister  = document.getElementById('navRegisterBtn');
    const navUserName  = document.getElementById('navUserName');
    const navFullName  = document.getElementById('navFullName');
    const navRole      = document.getElementById('navRole');
    const navAvatar    = document.getElementById('navAvatar');

    if (token && hoTen) {
        navLoggedIn?.classList.remove('d-none');
        navLoggedOut?.classList.add('d-none');
        navRegister?.classList.add('d-none');

        // Hiển thị tên / avatar
        if (navUserName) navUserName.textContent = hoTen.split(' ').pop();
        if (navFullName) navFullName.textContent = hoTen;
        if (navAvatar) {
            if (anhDaiDien && anhDaiDien !== 'undefined' && anhDaiDien !== '') {
                navAvatar.innerHTML = `<img src="${anhDaiDien}" class="rounded-circle" style="width:28px;height:28px;object-fit:cover" alt="avatar">`;
                navAvatar.style.cssText = 'background:transparent;color:inherit;display:inline-flex;align-items:center;justify-content:center;width:28px;height:28px';
            } else {
                const words = hoTen.split(' ');
                navAvatar.textContent = words.length >= 2
                    ? (words[words.length - 2][0] + words[words.length - 1][0]).toUpperCase()
                    : hoTen[0].toUpperCase();
            }
        }

        const roleLabel = { 'Admin': '👑 Quản trị viên', 'GiangVien': '👨‍🏫 Giảng viên', 'User': '🎓 Học viên' };
        if (navRole) navRole.textContent = roleLabel[vaiTro] || vaiTro;

        // Nav links & dropdown theo vai trò
        const linkDashboard  = document.getElementById('navMenuDashboard');
        const linkHoSoHV     = document.getElementById('linkHoSoHV');
        const linkHoSoGV     = document.getElementById('linkHoSoGV');
        const liLichHoc      = document.getElementById('navMenuLichHoc');
        const liKetQua       = document.getElementById('navMenuKetQua');

        const liKhoaHoc = document.getElementById('navMenuKhoaHoc');

        if (vaiTro === 'Admin') {
            navAdmin?.classList.remove('d-none');
            // Admin không cần "Học tập" hay "Lớp dạy"
            if (linkDashboard) linkDashboard.setAttribute('href', '/Admin');
            linkHoSoHV?.classList.add('d-none');
            liLichHoc?.classList.add('d-none');
            liKetQua?.classList.add('d-none');
            liKhoaHoc?.classList.add('d-none');

        } else if (vaiTro === 'GiangVien') {
            navGV?.classList.remove('d-none');
            if (linkDashboard) linkDashboard.setAttribute('href', '/GiangVien/Dashboard');
            linkHoSoHV?.classList.add('d-none');
            linkHoSoGV?.classList.remove('d-none');
            liKetQua?.classList.add('d-none');   // Giảng viên không có Kết quả học tập
            liKhoaHoc?.classList.add('d-none');

        } else {
            // User / Học viên
            navHV?.classList.remove('d-none');
            if (linkDashboard) linkDashboard.setAttribute('href', '/HocVien/Dashboard');
        }

    } else {
        navLoggedIn?.classList.add('d-none');
        navLoggedOut?.classList.remove('d-none');
        navRegister?.classList.remove('d-none');
    }

    // Đăng xuất
    document.getElementById('btnLogout')?.addEventListener('click', function(e) {
        e.preventDefault();
        ['jwt_token','ho_ten','email','vai_tro','anh_dai_dien'].forEach(k => localStorage.removeItem(k));
        // Xóa cookie
        document.cookie = 'jwt_token=; path=/; max-age=0; SameSite=Lax';
        window.location.href = '/';
    });
})();

// ─── TOAST TOÀN CỤC ─────────────────────────────────────────────────
function showToast(type, message) {
    let container = document.getElementById('toastContainer');
    if (!container) {
        container = document.createElement('div');
        container.id = 'toastContainer';
        document.body.appendChild(container);
    }
    const icons = { success:'bi-check-circle-fill', danger:'bi-x-circle-fill', warning:'bi-exclamation-triangle-fill', info:'bi-info-circle-fill' };
    const toast = document.createElement('div');
    toast.className = `toast align-items-center text-white bg-${type} border-0 show mb-2`;
    toast.setAttribute('role','alert');
    toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body d-flex align-items-center gap-2">
                <i class="bi ${icons[type]||'bi-info-circle-fill'}"></i>${message}
            </div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" onclick="this.closest('.toast').remove()"></button>
        </div>`;
    container.appendChild(toast);
    setTimeout(() => toast.remove(), 4500);
}

// ─── API HELPER ──────────────────────────────────────────────────────
async function apiCall(url, options = {}) {
    const token = localStorage.getItem('jwt_token');
    const headers = { ...(options.headers || {}) };
    if (token) headers['Authorization'] = 'Bearer ' + token;
    if (options.body && typeof options.body === 'object') {
        headers['Content-Type'] = 'application/json';
        options.body = JSON.stringify(options.body);
    }
    const res = await fetch(url, { ...options, headers });
    // Nếu 401 và không phải trang auth → logout tự động
    if (res.status === 401) {
        const path = window.location.pathname.toLowerCase();
        if (!path.startsWith('/auth') && !path.startsWith('/dang-')) {
            ['jwt_token','ho_ten','email','vai_tro','anh_dai_dien'].forEach(k => localStorage.removeItem(k));
            document.cookie = 'jwt_token=; path=/; max-age=0; SameSite=Lax';
            window.location.href = '/Auth/DangNhap';
        }
    }
    return res;
}
