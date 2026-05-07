// Admin Dashboard
let chartDoanhThu, chartNgonNgu, chartDiem;

// ─── UTILITY HELPERS ────────────────────────────────────────────────
function formatCurrency(v) {
    return v != null ? parseInt(v).toLocaleString('vi-VN') + '₫' : '—';
}
function formatDate(d) {
    return d ? new Date(d).toLocaleDateString('vi-VN') : '—';
}
// NOTE: trangThaiLabel is defined in site.js – do not redeclare here

function avatarHtml(anhDaiDien, hoTen, size = 36) {
    if (anhDaiDien) {
        return `<img src="${anhDaiDien}" class="rounded-circle" style="width:${size}px;height:${size}px;object-fit:cover" alt="${hoTen}">`;
    }
    const initials = (hoTen||'?').split(' ').pop()[0].toUpperCase();
    return `<div class="rounded-circle bg-secondary text-white d-inline-flex align-items-center justify-content-center fw-bold" style="width:${size}px;height:${size}px;font-size:${Math.round(size*0.4)}px">${initials}</div>`;
}

document.addEventListener('DOMContentLoaded', function() {
    const token = localStorage.getItem('jwt_token');
    const vaiTro = localStorage.getItem('vai_tro');
    if (!token || vaiTro !== 'Admin') { window.location.href = '/Auth/DangNhap'; return; }
    document.getElementById('lastUpdate').textContent = new Date().toLocaleString('vi-VN');
    loadAll();

    document.querySelectorAll('#adminTabs a').forEach(tab => {
        tab.addEventListener('shown.bs.tab', function(e) {
            const t = e.target.getAttribute('href');
            if (t === '#tabHocVien') loadHocVien();
            if (t === '#tabGiangVien') loadGiangVien();
            if (t === '#tabDangKy') loadDangKy();
        });
    });

    // Diem preview
    document.getElementById('diemGiuaKy')?.addEventListener('input', updateDiemPreview);
    document.getElementById('diemCuoiKy')?.addEventListener('input', updateDiemPreview);
});

async function loadAll() {
    await Promise.all([
        loadStats(), loadCharts(),
        loadKhoaHoc(), loadHocVien(), loadGiangVien(), loadDangKy(),
        loadKhoaHocSelect()
    ]);
}

async function loadStats() {
    try {
        const res = await apiCall('/api/ai/thong-ke');
        const d = await res.json();
        document.getElementById('statKhoaHoc').textContent = d.tongKhoaHoc;
        document.getElementById('statHocVien').textContent = d.tongHocVien;
        document.getElementById('statGiangVien').textContent = d.tongGiangVien;
        document.getElementById('statDangKy').textContent = d.tongDangKy;
        const choDuyet = d.dangKyChoDuyet || 0;
        document.getElementById('statChoDuyet').textContent = choDuyet;
        document.getElementById('statDoanhThu').textContent = (d.doanhThu / 1e6).toFixed(1) + 'M';

        // Badge chờ duyệt
        const badge = document.getElementById('badgeChoDuyet');
        if (badge) {
            if (choDuyet > 0) badge.classList.remove('d-none');
            else badge.classList.add('d-none');
        }

        // Khóa học phổ biến
        const el = document.getElementById('khoaHocPhoBien');
        if (d.khoaHocPhoBien?.length) {
            const max = d.khoaHocPhoBien[0].soLuong;
            el.innerHTML = d.khoaHocPhoBien.map((k, i) => `
                <div class="mb-3">
                    <div class="d-flex justify-content-between small mb-1">
                        <span class="fw-semibold">${i+1}. ${k.tenKhoaHoc}</span>
                        <span class="text-primary fw-bold">${k.soLuong} HV</span>
                    </div>
                    <div class="progress" style="height:8px">
                        <div class="progress-bar bg-${['primary','success','info','warning','danger'][i]}" style="width:${Math.round(k.soLuong/max*100)}%"></div>
                    </div>
                </div>`).join('');
        }
    } catch(e) { console.error(e); }
}

async function loadCharts() {
    try {
        const res = await apiCall('/api/ai/bao-cao');
        const d = await res.json();

        const ctx1 = document.getElementById('chartDoanhThu')?.getContext('2d');
        if (ctx1) {
            if (chartDoanhThu) chartDoanhThu.destroy();
            chartDoanhThu = new Chart(ctx1, {
                type: 'bar',
                data: {
                    labels: d.doanhThuTheoThang?.map(x => x.thang) || [],
                    datasets: [
                        { label: 'Doanh thu (triệu ₫)', data: d.doanhThuTheoThang?.map(x => (x.doanhThu/1e6).toFixed(1)) || [], backgroundColor: 'rgba(13,110,253,0.7)', borderColor: '#0d6efd', borderWidth: 2, borderRadius: 6, yAxisID: 'y' },
                        { label: 'Học viên mới', data: d.hocVienMoiTheoThang?.map(x => x.soLuong) || [], type: 'line', borderColor: '#198754', backgroundColor: 'rgba(25,135,84,0.1)', borderWidth: 2, tension: 0.4, pointRadius: 5, fill: true, yAxisID: 'y1' }
                    ]
                },
                options: { responsive: true, interaction: { mode: 'index', intersect: false }, plugins: { legend: { position: 'top' } }, scales: { y: { type:'linear', position:'left', title:{ display:true, text:'Triệu ₫' } }, y1: { type:'linear', position:'right', title:{ display:true, text:'Học viên' }, grid:{ drawOnChartArea:false } } } }
            });
        }

        const ctx2 = document.getElementById('chartNgonNgu')?.getContext('2d');
        if (ctx2) {
            if (chartNgonNgu) chartNgonNgu.destroy();
            const colors = { 'Tiếng Anh':'#0d6efd','Tiếng Nhật':'#dc3545','Tiếng Hàn':'#fd7e14','Tiếng Trung':'#198754','Tiếng Pháp':'#6f42c1' };
            chartNgonNgu = new Chart(ctx2, {
                type: 'doughnut',
                data: { labels: d.theoNgonNgu?.map(x => x.ngonNgu) || [], datasets: [{ data: d.theoNgonNgu?.map(x => x.soKhoaHoc) || [], backgroundColor: d.theoNgonNgu?.map(x => colors[x.ngonNgu] || '#6c757d') || [], borderWidth: 2, borderColor: '#fff' }] },
                options: { responsive: true, plugins: { legend: { position:'bottom', labels:{ font:{ size:11 } } } }, cutout: '60%' }
            });
        }

        const ctx3 = document.getElementById('chartDiem')?.getContext('2d');
        if (ctx3 && d.diemPhanPhoi) {
            if (chartDiem) chartDiem.destroy();
            const order = ['Xuất sắc','Giỏi','Khá','Trung bình','Yếu'];
            const sortedDiem = order.map(xl => ({ xepLoai: xl, soLuong: d.diemPhanPhoi.find(x => x.xepLoai === xl)?.soLuong || 0 })).filter(x => x.soLuong > 0);
            const diemColors = { 'Xuất sắc':'#198754','Giỏi':'#0d6efd','Khá':'#0dcaf0','Trung bình':'#ffc107','Yếu':'#dc3545' };
            chartDiem = new Chart(ctx3, {
                type: 'bar',
                data: { labels: sortedDiem.map(x => x.xepLoai), datasets: [{ label: 'Số học viên', data: sortedDiem.map(x => x.soLuong), backgroundColor: sortedDiem.map(x => diemColors[x.xepLoai] || '#6c757d'), borderRadius: 6 }] },
                options: { responsive: true, plugins: { legend: { display: false } }, scales: { y: { beginAtZero: true, ticks: { stepSize: 1 } } } }
            });
        }
    } catch(e) {
        console.error('loadCharts error:', e);
        ['chartDoanhThu','chartNgonNgu','chartDiem'].forEach(id => {
            const canvas = document.getElementById(id);
            if (canvas) {
                const parent = canvas.closest('.card-body');
                if (parent) parent.innerHTML = '<div class="text-center text-muted py-4 small"><i class="bi bi-bar-chart-line fs-2 d-block mb-2 opacity-25"></i>Không có dữ liệu</div>';
            }
        });
    }
}

async function loadKhoaHoc() {
    const search = document.getElementById('searchKhoaHoc')?.value || '';
    try {
        const res = await apiCall(`/api/khoa-hoc?search=${encodeURIComponent(search)}`);
        const data = await res.json();
        const tbody = document.getElementById('tableKhoaHoc');
        if (!data.length) { tbody.innerHTML = '<tr><td colspan="8" class="text-center text-muted py-3">Không có dữ liệu</td></tr>'; return; }
        tbody.innerHTML = data.map(k => `
            <tr>
                <td><code class="small">${k.maKhoaHoc}</code></td>
                <td class="fw-semibold small">${k.tenKhoaHoc}</td>
                <td><small>${k.ngonNgu}</small></td>
                <td><small>${k.trinhDo}</small></td>
                <td class="text-primary fw-semibold small">${formatCurrency(k.hocPhi)}</td>
                <td class="text-center"><span class="badge bg-${k.soHocVienHienTai >= k.soLuongToiDa*0.8 ? 'danger' : 'success'}">${k.soHocVienHienTai}/${k.soLuongToiDa}</span></td>
                <td>${trangThaiLabel[k.trangThai] || k.trangThai}</td>
                <td>
                    <div class="btn-group btn-group-sm">
                        <button class="btn btn-outline-warning" onclick="doiTrangThai(${k.id})" title="Đổi trạng thái"><i class="bi bi-arrow-repeat"></i></button>
                        <button class="btn btn-outline-danger" onclick="xoaKhoaHoc(${k.id})" title="Xóa"><i class="bi bi-trash"></i></button>
                    </div>
                </td>
            </tr>`).join('');
    } catch(e) {
        document.getElementById('tableKhoaHoc').innerHTML = `<tr><td colspan="8" class="text-center text-danger py-3"><i class="bi bi-exclamation-triangle me-1"></i>Lỗi tải dữ liệu. <a href="#" onclick="loadKhoaHoc()">Thử lại</a></td></tr>`;
    }
}

async function loadHocVien() {
    const search = document.getElementById('searchHocVien')?.value || '';
    try {
        const res = await apiCall(`/api/hoc-vien?search=${encodeURIComponent(search)}`);
        const data = await res.json();
        const tbody = document.getElementById('tableHocVien');
        if (!data.length) { tbody.innerHTML = '<tr><td colspan="10" class="text-center text-muted py-3">Không có dữ liệu</td></tr>'; return; }
        tbody.innerHTML = data.map(h => `
            <tr>
                <td>${avatarHtml(h.anhDaiDien, h.hoTen, 36)}</td>
                <td><code class="small">${h.maHocVien}</code></td>
                <td><div class="fw-semibold small">${h.hoTen}</div></td>
                <td><small class="text-muted">${h.email}</small></td>
                <td><small>${h.soDienThoai || '—'}</small></td>
                <td><small>${h.ngaySinh ? new Date(h.ngaySinh).toLocaleDateString('vi-VN') : '—'}</small></td>
                <td><small>${h.gioiTinh || '—'}</small></td>
                <td><small>${h.trinhDoHienTai || '—'}</small></td>
                <td class="text-center"><span class="badge bg-primary">${h.soKhoaHocDangHoc}</span></td>
                <td>
                    <div class="btn-group btn-group-sm">
                        <button class="btn btn-outline-primary" onclick="xemHocVien(${h.id})" title="Chi tiết"><i class="bi bi-eye"></i></button>
                        <button class="btn btn-outline-secondary" onclick="moUploadAvatar(${h.nguoiDungId}, '${h.anhDaiDien||''}', '${h.hoTen}')" title="Ảnh đại diện"><i class="bi bi-camera"></i></button>
                    </div>
                </td>
            </tr>`).join('');
    } catch(e) {
        document.getElementById('tableHocVien').innerHTML = `<tr><td colspan="10" class="text-center text-danger py-3"><i class="bi bi-exclamation-triangle me-1"></i>Lỗi tải dữ liệu. <a href="#" onclick="loadHocVien()">Thử lại</a></td></tr>`;
    }
}

async function loadGiangVien() {
    const search = document.getElementById('searchGiangVien')?.value || '';
    try {
        const res = await apiCall(`/api/giang-vien?search=${encodeURIComponent(search)}`);
        const data = await res.json();
        const tbody = document.getElementById('tableGiangVien');
        if (!data.length) { tbody.innerHTML = '<tr><td colspan="11" class="text-center text-muted py-3">Không có dữ liệu</td></tr>'; return; }
        tbody.innerHTML = data.map(g => `
            <tr>
                <td>${avatarHtml(g.anhDaiDien, g.hoTen, 36)}</td>
                <td><code class="small">${g.maGiangVien}</code></td>
                <td><div class="fw-semibold small">${g.hoTen}</div></td>
                <td><small class="text-muted">${g.email}</small></td>
                <td><small>${g.soDienThoai || '—'}</small></td>
                <td><small>${g.chuyenNganh}</small></td>
                <td><small>${g.bangCap}</small></td>
                <td class="text-center"><small>${g.namKinhNghiem} năm</small></td>
                <td class="text-center"><span class="badge bg-info">${g.soKhoaHocPhuTrach}</span></td>
                <td><span class="badge ${g.dangHoatDong ? 'bg-success' : 'bg-secondary'}">${g.dangHoatDong ? 'Hoạt động' : 'Ngừng'}</span></td>
                <td>
                    <button class="btn btn-sm btn-outline-secondary" onclick="moUploadAvatar(${g.nguoiDungId || g.id}, '${g.anhDaiDien||''}', '${g.hoTen}')" title="Ảnh đại diện"><i class="bi bi-camera"></i></button>
                </td>
            </tr>`).join('');
    } catch(e) {
        document.getElementById('tableGiangVien').innerHTML = `<tr><td colspan="11" class="text-center text-danger py-3"><i class="bi bi-exclamation-triangle me-1"></i>Lỗi tải dữ liệu. <a href="#" onclick="loadGiangVien()">Thử lại</a></td></tr>`;
    }
}

async function loadDangKy() {
    const ts = document.getElementById('filterTrangThaiDK')?.value || '';
    try {
        const res = await apiCall(`/api/dang-ky?trangThai=${ts}`);
        const data = await res.json();
        const tbody = document.getElementById('tableDangKy');
        if (!data.length) { tbody.innerHTML = '<tr><td colspan="7" class="text-center text-muted py-3">Không có đăng ký</td></tr>'; return; }
        tbody.innerHTML = data.map(d => `
            <tr class="${d.trangThai === 'ChoDuyet' ? 'table-warning' : ''}">
                <td class="small"><span class="fw-semibold">${d.tenHocVien}</span><br><code style="font-size:.7rem">${d.maHocVien}</code></td>
                <td class="small">${d.tenKhoaHoc}</td>
                <td class="small">${formatDate(d.ngayDangKy)}</td>
                <td class="small">${formatCurrency(d.hocPhi)}</td>
                <td class="small ${d.soTienDaThanhToan > 0 ? 'text-success' : 'text-danger'}">${formatCurrency(d.soTienDaThanhToan)}</td>
                <td>${trangThaiLabel[d.trangThai] || d.trangThai}</td>
                <td>
                    <div class="btn-group btn-group-sm">
                        ${d.trangThai === 'ChoDuyet' ? `
                            <button class="btn btn-success btn-sm" onclick="duyetDangKy(${d.id})" title="Duyệt"><i class="bi bi-check-lg"></i></button>
                            <button class="btn btn-danger btn-sm" onclick="tuChoiDangKy(${d.id})" title="Từ chối"><i class="bi bi-x-lg"></i></button>
                        ` : `<button class="btn btn-outline-danger btn-sm" onclick="huyDangKy(${d.id})" title="Hủy"><i class="bi bi-x-lg"></i></button>`}
                    </div>
                </td>
            </tr>`).join('');
    } catch(e) {
        document.getElementById('tableDangKy').innerHTML = `<tr><td colspan="7" class="text-center text-danger py-3"><i class="bi bi-exclamation-triangle me-1"></i>Lỗi tải dữ liệu. <a href="#" onclick="loadDangKy()">Thử lại</a></td></tr>`;
    }
}

function switchToChoDuyet() {
    // Switch to đăng ký tab with filter chờ duyệt
    const tab = document.querySelector('#adminTabs a[href="#tabDangKy"]');
    bootstrap.Tab.getOrCreateInstance(tab).show();
    setTimeout(() => filterChoDuyet(), 100);
}

function filterChoDuyet() {
    document.getElementById('filterTrangThaiDK').value = 'ChoDuyet';
    loadDangKy();
}

async function loadKhoaHocSelect() {
    try {
        const res = await apiCall('/api/khoa-hoc');
        const data = await res.json();
        const sel = document.getElementById('selectKhoaHocDiem');
        if (sel) data.forEach(k => sel.innerHTML += `<option value="${k.id}">${k.tenKhoaHoc}</option>`);
    } catch(e) {}
}

async function loadDiemTheoKhoaHoc() {
    const id = document.getElementById('selectKhoaHocDiem').value;
    const btnXuat = document.getElementById('btnXuatDiem');
    if (btnXuat) btnXuat.style.display = id ? '' : 'none';
    if (!id) return;
    try {
        const res = await apiCall(`/api/diem/khoa-hoc/${id}`);
        const data = await res.json();
        const tbody = document.getElementById('tableDiem');
        if (!data.length) { tbody.innerHTML = '<tr><td colspan="8" class="text-center text-muted py-3">Chưa có điểm</td></tr>'; return; }
        const xepLoaiColor = { 'Xuất sắc':'success','Giỏi':'primary','Khá':'info','Trung bình':'warning','Không đạt':'danger' };
        tbody.innerHTML = data.map(d => `
            <tr>
                <td class="fw-semibold small">${d.tenHocVien}</td>
                <td><code class="small">${d.maHocVien}</code></td>
                <td class="text-center">${d.diemChuyenCan != null ? d.diemChuyenCan.toFixed(1) : '—'}</td>
                <td class="text-center">${d.diemGiuaKy != null ? d.diemGiuaKy.toFixed(1) : '—'}</td>
                <td class="text-center">${d.diemCuoiKy != null ? d.diemCuoiKy.toFixed(1) : '—'}</td>
                <td class="text-center fw-bold ${d.diemTrungBinh >= 80 ? 'text-success' : d.diemTrungBinh < 50 ? 'text-danger' : ''}">${d.diemTrungBinh != null ? d.diemTrungBinh.toFixed(1) : '—'}</td>
                <td class="text-center">${d.xepLoai ? `<span class="badge bg-${xepLoaiColor[d.xepLoai]||'secondary'}">${d.xepLoai}</span>` : '—'}</td>
                <td>
                    <button class="btn btn-sm btn-outline-primary" onclick="moCapNhatDiem(${d.id},'${d.tenHocVien}',${d.diemGiuaKy??'null'},${d.diemCuoiKy??'null'},'${(d.nhanXet||'').replace(/'/g,"\\'")}',${d.daHoanThanh})">
                        <i class="bi bi-pencil"></i>
                    </button>
                </td>
            </tr>`).join('');
    } catch(e) { showToast('danger', 'Lỗi tải điểm'); }
}

function xuatDiem() {
    const id = document.getElementById('selectKhoaHocDiem').value;
    if (!id) { showToast('warning', 'Vui lòng chọn khóa học trước'); return; }

    // Collect data from table
    const rows = document.querySelectorAll('#tableDiem tr');
    const data = [['Học viên', 'Mã HV', 'Chuyên cần', 'Giữa kỳ', 'Cuối kỳ', 'Tổng kết', 'Xếp loại']];
    rows.forEach(r => {
        const cells = r.querySelectorAll('td');
        if (cells.length > 6) {
            data.push(Array.from(cells).slice(0, 7).map(c => c.textContent.trim()));
        }
    });

    if (data.length <= 1) { showToast('warning', 'Không có dữ liệu điểm để xuất'); return; }

    // Use SheetJS to create proper xlsx
    const wb = XLSX.utils.book_new();
    const ws = XLSX.utils.aoa_to_sheet(data);

    // Column widths
    ws['!cols'] = [
        { wch: 25 }, { wch: 10 }, { wch: 12 }, { wch: 10 },
        { wch: 10 }, { wch: 10 }, { wch: 12 }
    ];

    // Style header row (SheetJS CE doesn't support rich styles, but xlsx-js-style does)
    XLSX.utils.book_append_sheet(wb, ws, 'Bảng điểm');

    // Add metadata sheet
    const tenKH = document.getElementById('selectKhoaHocDiem').options[document.getElementById('selectKhoaHocDiem').selectedIndex]?.text || `KH${id}`;
    const metaData = [
        ['Khóa học:', tenKH],
        ['Xuất ngày:', new Date().toLocaleDateString('vi-VN')],
        ['Tổng học viên:', data.length - 1]
    ];
    const wsMeta = XLSX.utils.aoa_to_sheet(metaData);
    XLSX.utils.book_append_sheet(wb, wsMeta, 'Thông tin');

    XLSX.writeFile(wb, `bang_diem_${id}_${new Date().toISOString().slice(0,10)}.xlsx`);
    showToast('success', `Đã xuất ${data.length - 1} học viên ra file Excel`);
}

async function luuGiangVien() {
    const hoTen = document.getElementById('gvHoTen').value.trim();
    const email = document.getElementById('gvEmail').value.trim();
    const matKhau = document.getElementById('gvMatKhau').value;
    const soDienThoai = document.getElementById('gvSoDienThoai').value.trim();
    const maGiangVien = document.getElementById('gvMaGiangVien').value.trim();
    const chuyenNganh = document.getElementById('gvChuyenNganh').value.trim();
    const bangCap = document.getElementById('gvBangCap').value;
    const namKinhNghiem = parseInt(document.getElementById('gvNamKinhNghiem').value) || 0;
    const gioiThieu = document.getElementById('gvGioiThieu').value.trim();

    if (!hoTen || !email || !matKhau || !maGiangVien) {
        showToast('warning', 'Vui lòng điền đầy đủ các trường bắt buộc (*)');
        return;
    }
    if (matKhau.length < 6) {
        showToast('warning', 'Mật khẩu phải có ít nhất 6 ký tự');
        return;
    }

    const btn = document.querySelector('#modalTaoGV .btn-warning');
    const origText = btn.innerHTML;
    btn.disabled = true;
    btn.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span>Đang lưu...';

    try {
        const res = await apiCall('/api/giang-vien', {
            method: 'POST',
            body: { hoTen, email, matKhau, soDienThoai, maGiangVien, chuyenNganh, bangCap, namKinhNghiem, gioiThieu }
        });
        const data = await res.json();
        if (res.ok) {
            showToast('success', `Đã thêm giảng viên ${hoTen} thành công!`);
            bootstrap.Modal.getInstance(document.getElementById('modalTaoGV')).hide();
            // Reset form
            ['gvHoTen','gvEmail','gvMatKhau','gvSoDienThoai','gvMaGiangVien','gvChuyenNganh','gvGioiThieu'].forEach(id => {
                document.getElementById(id).value = '';
            });
            document.getElementById('gvBangCap').value = '';
            document.getElementById('gvNamKinhNghiem').value = '0';
            loadGiangVien();
            loadStats();
        } else {
            showToast('danger', data.message || 'Lỗi thêm giảng viên');
        }
    } catch(e) {
        showToast('danger', 'Lỗi kết nối server');
    } finally {
        btn.disabled = false;
        btn.innerHTML = origText;
    }
}

// ─── BANNER ─────────────────────────────────────────────────────────
async function loadBanners() {
    try {
        const res = await apiCall('/api/banner/all');
        const data = await res.json();
        const grid = document.getElementById('bannerGrid');
        if (!data.length) {
            grid.innerHTML = '<div class="col-12 text-center text-muted py-4"><i class="bi bi-image fs-1 d-block mb-2"></i>Chưa có banner nào</div>';
            return;
        }
        grid.innerHTML = data.map(b => `
            <div class="col-md-6 col-lg-4">
                <div class="card border-0 shadow-sm h-100 ${!b.dangHienThi ? 'opacity-50' : ''}">
                    <div class="position-relative">
                        <img src="${b.hinhAnh}" class="card-img-top" style="height:160px;object-fit:cover"
                            onerror="this.src='https://via.placeholder.com/400x160?text=No+Image'" alt="${b.tieuDe}">
                        <span class="position-absolute top-0 end-0 m-2 badge ${b.dangHienThi ? 'bg-success' : 'bg-secondary'}">#${b.thuTu} ${b.dangHienThi ? '✓' : '✗'}</span>
                    </div>
                    <div class="card-body p-3">
                        <h6 class="fw-bold mb-1">${b.tieuDe}</h6>
                        <p class="text-muted small mb-2">${b.moTa || '—'}</p>
                        ${b.duongDanLienKet ? `<small class="text-primary"><i class="bi bi-link-45deg me-1"></i>${b.duongDanLienKet}</small>` : ''}
                    </div>
                    <div class="card-footer bg-transparent border-0 d-flex gap-2 pb-3 px-3">
                        <button class="btn btn-sm btn-outline-primary flex-fill" onclick="moModalBanner(${JSON.stringify(b).replace(/"/g,'&quot;')})">
                            <i class="bi bi-pencil me-1"></i>Sửa
                        </button>
                        <button class="btn btn-sm ${b.dangHienThi ? 'btn-outline-warning' : 'btn-outline-success'} flex-fill" onclick="toggleBanner(${b.id})">
                            ${b.dangHienThi ? '<i class="bi bi-eye-slash me-1"></i>Ẩn' : '<i class="bi bi-eye me-1"></i>Hiện'}
                        </button>
                        <button class="btn btn-sm btn-outline-danger" onclick="xoaBanner(${b.id})" title="Xóa">
                            <i class="bi bi-trash"></i>
                        </button>
                    </div>
                </div>
            </div>`).join('');
    } catch(e) { showToast('danger', 'Lỗi tải banner'); }
}

function moModalBanner(bannerJson) {
    let b = null;
    if (typeof bannerJson === 'string') {
        try { b = JSON.parse(bannerJson.replace(/&quot;/g, '"')); } catch(e) {}
    } else if (bannerJson && typeof bannerJson === 'object') {
        b = bannerJson;
    }
    document.getElementById('bannerId').value = b?.id || '';
    document.getElementById('bannerModalTitle').innerHTML = b
        ? '<i class="bi bi-pencil me-2"></i>Sửa banner' : '<i class="bi bi-plus-circle me-2"></i>Thêm banner';
    document.getElementById('bannerTieuDe').value = b?.tieuDe || '';
    document.getElementById('bannerMoTa').value = b?.moTa || '';
    document.getElementById('bannerHinhAnh').value = b?.hinhAnh || '';
    document.getElementById('bannerLienKet').value = b?.duongDanLienKet || '';
    document.getElementById('bannerThuTu').value = b?.thuTu || 1;
    document.getElementById('bannerHienThi').checked = b?.dangHienThi !== false;
    previewBanner();
    new bootstrap.Modal(document.getElementById('modalBanner')).show();
}

function previewBanner() {
    const url = document.getElementById('bannerHinhAnh')?.value;
    const wrap = document.getElementById('bannerPreviewWrap');
    const img = document.getElementById('bannerPreviewImg');
    if (url && url.startsWith('http')) { img.src = url; wrap.style.display = ''; }
    else wrap.style.display = 'none';
}

async function luuBanner() {
    const id = document.getElementById('bannerId').value;
    const body = {
        tieuDe: document.getElementById('bannerTieuDe').value.trim(),
        moTa: document.getElementById('bannerMoTa').value.trim(),
        hinhAnh: document.getElementById('bannerHinhAnh').value.trim(),
        duongDanLienKet: document.getElementById('bannerLienKet').value.trim(),
        thuTu: parseInt(document.getElementById('bannerThuTu').value) || 1,
        dangHienThi: document.getElementById('bannerHienThi').checked
    };
    if (!body.tieuDe || !body.hinhAnh) { showToast('warning', 'Vui lòng nhập tiêu đề và URL ảnh'); return; }
    const url = id ? `/api/banner/${id}` : '/api/banner';
    const method = id ? 'PUT' : 'POST';
    const res = await apiCall(url, { method, body });
    if (res.ok) {
        showToast('success', id ? '✅ Cập nhật banner thành công!' : '✅ Thêm banner thành công!');
        bootstrap.Modal.getInstance(document.getElementById('modalBanner')).hide();
        loadBanners();
    } else showToast('danger', 'Lưu banner thất bại');
}

async function toggleBanner(id) {
    const res = await apiCall(`/api/banner/${id}/toggle`, { method: 'PATCH' });
    if (res.ok) loadBanners();
    else showToast('danger', 'Cập nhật thất bại');
}

async function xoaBanner(id) {
    if (!confirm('Bạn có chắc muốn xóa banner này?')) return;
    const res = await apiCall(`/api/banner/${id}`, { method: 'DELETE' });
    const d = await res.json().catch(() => ({}));
    showToast(res.ok ? 'success' : 'danger', d.message || (res.ok ? 'Đã xóa' : 'Xóa thất bại'));
    if (res.ok) loadBanners();
}

// ─── BÀI VIẾT ───────────────────────────────────────────────────────
async function loadBaiViet() {
    const theLoai = document.getElementById('filterTheLoaiBV')?.value || '';
    try {
        const res = await apiCall(`/api/bai-viet/all`);
        const data = await res.json();
        const grid = document.getElementById('baiVietGrid');
        let filtered = theLoai ? data.filter(b => b.theLoai === theLoai) : data;
        if (!filtered.length) {
            grid.innerHTML = '<div class="col-12 text-center text-muted py-4"><i class="bi bi-newspaper fs-1 d-block mb-2"></i>Chưa có bài viết nào</div>';
            return;
        }
        grid.innerHTML = filtered.map(b => `
            <div class="col-md-6 col-lg-4">
                <div class="card border-0 shadow-sm h-100 ${!b.dangHienThi ? 'opacity-60' : ''}">
                    ${b.hinhAnh ? `<img src="${b.hinhAnh}" class="card-img-top" style="height:140px;object-fit:cover" onerror="this.style.display='none'" alt="">` : ''}
                    <div class="card-body p-3">
                        <div class="d-flex gap-1 mb-2">
                            <span class="badge bg-secondary small">${b.theLoai}</span>
                            ${b.noiBat ? '<span class="badge bg-warning text-dark small">★ Nổi bật</span>' : ''}
                            ${!b.dangHienThi ? '<span class="badge bg-light text-dark border small">Đã ẩn</span>' : ''}
                        </div>
                        <h6 class="fw-bold mb-1" style="line-height:1.4">${b.tieuDe}</h6>
                        <p class="text-muted small mb-1" style="display:-webkit-box;-webkit-line-clamp:2;-webkit-box-orient:vertical;overflow:hidden">${b.tomTat || '—'}</p>
                        <small class="text-muted"><i class="bi bi-clock me-1"></i>${formatDate(b.ngayTao)} · <i class="bi bi-eye me-1"></i>${b.luotXem}</small>
                    </div>
                    <div class="card-footer bg-transparent border-0 d-flex gap-2 pb-3 px-3">
                        <button class="btn btn-sm btn-outline-primary flex-fill" onclick="moModalBaiViet(${JSON.stringify(b).replace(/"/g,'&quot;')})">
                            <i class="bi bi-pencil me-1"></i>Sửa
                        </button>
                        <button class="btn btn-sm ${b.dangHienThi ? 'btn-outline-warning' : 'btn-outline-success'} flex-fill" onclick="toggleBaiViet(${b.id})">
                            ${b.dangHienThi ? '<i class="bi bi-eye-slash me-1"></i>Ẩn' : '<i class="bi bi-eye me-1"></i>Hiện'}
                        </button>
                        <button class="btn btn-sm btn-outline-danger" onclick="xoaBaiViet(${b.id})"><i class="bi bi-trash"></i></button>
                    </div>
                </div>
            </div>`).join('');
    } catch(e) { showToast('danger', 'Lỗi tải bài viết'); }
}

function moModalBaiViet(bvJson) {
    let b = null;
    if (typeof bvJson === 'string') {
        try { b = JSON.parse(bvJson.replace(/&quot;/g, '"')); } catch(e) {}
    } else if (bvJson && typeof bvJson === 'object') {
        b = bvJson;
    }
    document.getElementById('baiVietId').value = b?.id || '';
    document.getElementById('baiVietModalTitle').innerHTML = b
        ? '<i class="bi bi-pencil me-2"></i>Sửa bài viết' : '<i class="bi bi-plus-circle me-2"></i>Thêm bài viết';
    document.getElementById('bvTieuDe').value = b?.tieuDe || '';
    document.getElementById('bvTheLoai').value = b?.theLoai || 'Tin tức';
    document.getElementById('bvTacGia').value = b?.tacGia || '';
    document.getElementById('bvTomTat').value = b?.tomTat || '';
    document.getElementById('bvHinhAnh').value = b?.hinhAnh || '';
    document.getElementById('bvNoiDung').value = b?.noiDung || '';
    document.getElementById('bvHienThi').checked = b?.dangHienThi !== false;
    document.getElementById('bvNoiBat').checked = b?.noiBat === true;
    previewBaiViet();
    new bootstrap.Modal(document.getElementById('modalBaiViet')).show();
}

function previewBaiViet() {
    const url = document.getElementById('bvHinhAnh')?.value;
    const wrap = document.getElementById('bvPreviewWrap');
    const img = document.getElementById('bvPreviewImg');
    if (url && url.startsWith('http')) { img.src = url; wrap.style.display = ''; }
    else wrap.style.display = 'none';
}

async function luuBaiViet() {
    const id = document.getElementById('baiVietId').value;
    const body = {
        tieuDe: document.getElementById('bvTieuDe').value.trim(),
        theLoai: document.getElementById('bvTheLoai').value,
        tacGia: document.getElementById('bvTacGia').value.trim(),
        tomTat: document.getElementById('bvTomTat').value.trim(),
        hinhAnh: document.getElementById('bvHinhAnh').value.trim(),
        noiDung: document.getElementById('bvNoiDung').value.trim(),
        dangHienThi: document.getElementById('bvHienThi').checked,
        noiBat: document.getElementById('bvNoiBat').checked
    };
    if (!body.tieuDe) { showToast('warning', 'Vui lòng nhập tiêu đề bài viết'); return; }
    const url = id ? `/api/bai-viet/${id}` : '/api/bai-viet';
    const method = id ? 'PUT' : 'POST';
    const res = await apiCall(url, { method, body });
    if (res.ok) {
        showToast('success', id ? '✅ Cập nhật bài viết thành công!' : '✅ Thêm bài viết thành công!');
        bootstrap.Modal.getInstance(document.getElementById('modalBaiViet')).hide();
        loadBaiViet();
    } else showToast('danger', 'Lưu bài viết thất bại');
}

async function toggleBaiViet(id) {
    const res = await apiCall(`/api/bai-viet/${id}/toggle`, { method: 'PATCH' });
    if (res.ok) loadBaiViet();
    else showToast('danger', 'Cập nhật thất bại');
}

async function xoaBaiViet(id) {
    if (!confirm('Bạn có chắc muốn xóa bài viết này?')) return;
    const res = await apiCall(`/api/bai-viet/${id}`, { method: 'DELETE' });
    const d = await res.json().catch(() => ({}));
    showToast(res.ok ? 'success' : 'danger', d.message || (res.ok ? 'Đã xóa' : 'Xóa thất bại'));
    if (res.ok) loadBaiViet();
}

// ─── HỌC VIÊN CHI TIẾT ──────────────────────────────────────────────
let _hvDetailId = null;   // nguoiDungId
let _hvId = null;         // HocVien.Id (dùng cho PUT /api/hoc-vien/{id})

async function xemHocVien(hvId) {
    _hvId = hvId;
    const body = document.getElementById('hvDetailBody');
    body.innerHTML = '<div class="text-center py-4"><div class="spinner-border spinner-border-sm"></div></div>';
    new bootstrap.Modal(document.getElementById('modalHocVien')).show();

    try {
        const res = await apiCall(`/api/hoc-vien/${hvId}`);
        const h = await res.json();
        if (!h || !h.id) { body.innerHTML = '<p class="text-danger">Không tìm thấy học viên</p>'; return; }
        _hvDetailId = h.nguoiDungId;

        body.innerHTML = `
            <div class="row g-3">
                <div class="col-md-3 text-center">
                    <div id="hvAvatarWrap">${avatarHtml(h.anhDaiDien, h.hoTen, 80)}</div>
                    <div class="mt-2">
                        <button class="btn btn-sm btn-outline-secondary" onclick="moUploadAvatar(${h.nguoiDungId}, '${h.anhDaiDien||''}', '${h.hoTen}')">
                            <i class="bi bi-camera me-1"></i>Đổi ảnh
                        </button>
                    </div>
                    <div class="mt-2"><code class="small">${h.maHocVien}</code></div>
                </div>
                <div class="col-md-9">
                    <div class="row g-2">
                        <div class="col-md-6">
                            <label class="form-label fw-semibold small">Họ tên</label>
                            <input type="text" class="form-control form-control-sm" id="hvDetailHoTen" value="${h.hoTen}">
                        </div>
                        <div class="col-md-6">
                            <label class="form-label fw-semibold small">Email</label>
                            <input type="email" class="form-control form-control-sm" id="hvDetailEmail" value="${h.email}" readonly>
                        </div>
                        <div class="col-md-6">
                            <label class="form-label fw-semibold small">SĐT</label>
                            <input type="text" class="form-control form-control-sm" id="hvDetailSdt" value="${h.soDienThoai||''}">
                        </div>
                        <div class="col-md-6">
                            <label class="form-label fw-semibold small">Trình độ</label>
                            <input type="text" class="form-control form-control-sm" id="hvDetailTrinhDo" value="${h.trinhDoHienTai||''}">
                        </div>
                        <div class="col-md-6">
                            <label class="form-label fw-semibold small">Ngày sinh</label>
                            <input type="date" class="form-control form-control-sm" id="hvDetailNgaySinh" value="${h.ngaySinh ? new Date(h.ngaySinh).toISOString().split('T')[0] : ''}">
                        </div>
                        <div class="col-md-6">
                            <label class="form-label fw-semibold small">Giới tính</label>
                            <select class="form-select form-select-sm" id="hvDetailGioiTinh">
                                <option ${h.gioiTinh==='Nam'?'selected':''}>Nam</option>
                                <option ${h.gioiTinh==='Nữ'?'selected':''}>Nữ</option>
                                <option ${!h.gioiTinh||h.gioiTinh==='Khác'?'selected':''}>Khác</option>
                            </select>
                        </div>
                        <div class="col-12">
                            <label class="form-label fw-semibold small">Địa chỉ</label>
                            <input type="text" class="form-control form-control-sm" id="hvDetailDiaChi" value="${h.diaChi||''}">
                        </div>
                    </div>
                </div>
            </div>
            <div class="mt-3">
                <p class="fw-semibold small mb-2"><i class="bi bi-book me-1"></i>Khóa học đang học: <span class="badge bg-primary">${h.soKhoaHocDangHoc}</span></p>
            </div>`;
    } catch(e) {
        body.innerHTML = '<p class="text-danger">Lỗi tải dữ liệu</p>';
    }
}

async function capNhatHocVien() {
    if (!_hvId) return;
    const body = {
        hoTen:         document.getElementById('hvDetailHoTen')?.value?.trim(),
        soDienThoai:   document.getElementById('hvDetailSdt')?.value?.trim(),
        ngaySinh:      document.getElementById('hvDetailNgaySinh')?.value || null,
        gioiTinh:      document.getElementById('hvDetailGioiTinh')?.value,
        trinhDoHienTai: document.getElementById('hvDetailTrinhDo')?.value?.trim(),
        diaChi:        document.getElementById('hvDetailDiaChi')?.value?.trim()
    };
    if (!body.hoTen) { showToast('warning', 'Họ tên không được để trống'); return; }

    const btn = document.querySelector('#modalHocVien .btn-primary');
    if (btn) { btn.disabled = true; btn.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span>Đang lưu...'; }

    try {
        const res = await apiCall(`/api/hoc-vien/${_hvId}`, { method: 'PUT', body });
        const d = await res.json().catch(() => ({}));
        if (res.ok) {
            showToast('success', '✅ Cập nhật thông tin học viên thành công!');
            bootstrap.Modal.getInstance(document.getElementById('modalHocVien'))?.hide();
            loadHocVien();
        } else {
            showToast('danger', d.message || 'Cập nhật thất bại');
        }
    } catch(e) {
        showToast('danger', 'Lỗi kết nối');
    } finally {
        if (btn) { btn.disabled = false; btn.innerHTML = '<i class="bi bi-check2 me-1"></i>Lưu thay đổi'; }
    }
}

// ─── AVATAR UPLOAD ───────────────────────────────────────────────────
function moUploadAvatar(userId, currentAvatar, hoTen) {
    document.getElementById('avatarUserId').value = userId;
    document.getElementById('avatarCurrentImg').src = currentAvatar || '/images/default-avatar.png';
    document.getElementById('avatarCurrentImg').onerror = function() { this.src = 'data:image/svg+xml,<svg xmlns="http://www.w3.org/2000/svg" width="100" height="100"><rect width="100" height="100" fill="%236c757d" rx="50"/><text x="50" y="65" font-size="40" text-anchor="middle" fill="white">' + (hoTen||'?').split(' ').pop()[0] + '</text></svg>'; };
    document.getElementById('avatarFile').value = '';
    document.getElementById('avatarPreviewWrap').classList.add('d-none');
    new bootstrap.Modal(document.getElementById('modalAvatar')).show();
}

function previewAvatar() {
    const file = document.getElementById('avatarFile').files[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = e => {
        document.getElementById('avatarNewPreview').src = e.target.result;
        document.getElementById('avatarPreviewWrap').classList.remove('d-none');
    };
    reader.readAsDataURL(file);
}

async function uploadAvatar() {
    const userId = document.getElementById('avatarUserId').value;
    const file = document.getElementById('avatarFile').files[0];
    if (!file) { showToast('warning', 'Vui lòng chọn ảnh'); return; }

    const formData = new FormData();
    formData.append('file', file);

    const token = localStorage.getItem('jwt_token');
    try {
        const res = await fetch(`/api/upload/avatar/${userId}`, {
            method: 'POST',
            headers: { 'Authorization': `Bearer ${token}` },
            body: formData
        });
        const d = await res.json();
        if (res.ok) {
            showToast('success', '✅ Tải ảnh lên thành công!');
            bootstrap.Modal.getInstance(document.getElementById('modalAvatar')).hide();
            loadHocVien();
            loadGiangVien();
        } else showToast('danger', d.message || 'Lỗi tải ảnh');
    } catch(e) { showToast('danger', 'Lỗi kết nối'); }
}

// ─── ACTIONS ─────────────────────────────────────────────────────────
async function luuKhoaHoc() {
    const form = document.getElementById('formTaoKhoaHoc');
    const fd = new FormData(form);
    const body = {
        maKhoaHoc: fd.get('maKhoaHoc'), tenKhoaHoc: fd.get('tenKhoaHoc'),
        moTa: fd.get('moTa'), ngonNgu: fd.get('ngonNgu'), trinhDo: fd.get('trinhDo'),
        hocPhi: parseFloat(fd.get('hocPhi')), soBuoiHoc: parseInt(fd.get('soBuoiHoc')),
        thoiLuongMoiBuoi: parseInt(fd.get('thoiLuongMoiBuoi')),
        soLuongToiDa: parseInt(fd.get('soLuongToiDa')),
        ngayBatDau: fd.get('ngayBatDau'), ngayKetThuc: fd.get('ngayKetThuc'), danhSachLichHoc: []
    };
    const res = await apiCall('/api/khoa-hoc', { method:'POST', body });
    const data = await res.json();
    if (res.ok) {
        showToast('success', 'Thêm khóa học thành công!');
        bootstrap.Modal.getInstance(document.getElementById('modalTaoKhoaHoc')).hide();
        form.reset(); loadKhoaHoc(); loadStats();
    } else showToast('danger', data.message || 'Lỗi thêm khóa học');
}

async function xoaKhoaHoc(id) {
    if (!confirm('Bạn có chắc muốn xóa khóa học này?')) return;
    const res = await apiCall(`/api/khoa-hoc/${id}`, { method:'DELETE' });
    const d = await res.json();
    showToast(res.ok ? 'success' : 'danger', d.message);
    if (res.ok) { loadKhoaHoc(); loadStats(); }
}

async function doiTrangThai(id) {
    const ts = prompt('Nhập trạng thái mới:\n1 = Mở đăng ký\n2 = Đang học\n3 = Đã kết thúc\n4 = Hủy bỏ');
    if (!ts || isNaN(ts)) return;
    const res = await apiCall(`/api/khoa-hoc/${id}/trang-thai`, { method:'PATCH', body: parseInt(ts) });
    const d = await res.json();
    showToast(res.ok ? 'success' : 'danger', d.message);
    if (res.ok) { loadKhoaHoc(); loadStats(); }
}

async function duyetDangKy(id) {
    const res = await apiCall(`/api/dang-ky/${id}/trang-thai`, { method:'PUT', body:{ trangThai:3, ghiChu:'Admin đã duyệt' } });
    const d = await res.json();
    showToast(res.ok ? 'success' : 'danger', d.message);
    if (res.ok) { loadDangKy(); loadStats(); }
}

async function tuChoiDangKy(id) {
    const lyDo = prompt('Lý do từ chối (tùy chọn):') || 'Không đủ điều kiện';
    const res = await apiCall(`/api/dang-ky/${id}/trang-thai`, { method:'PUT', body:{ trangThai:5, ghiChu: lyDo } });
    const d = await res.json();
    showToast(res.ok ? 'success' : 'danger', d.message);
    if (res.ok) { loadDangKy(); loadStats(); }
}

async function huyDangKy(id) {
    if (!confirm('Hủy đăng ký này?')) return;
    const res = await apiCall(`/api/dang-ky/${id}/trang-thai`, { method:'PUT', body:{ trangThai:5, ghiChu:'Admin hủy' } });
    const d = await res.json();
    showToast(res.ok ? 'success' : 'danger', d.message);
    if (res.ok) loadDangKy();
}

function updateDiemPreview() {
    const gk = parseFloat(document.getElementById('diemGiuaKy').value);
    const ck = parseFloat(document.getElementById('diemCuoiKy').value);
    const el = document.getElementById('diemPreview');
    if (!el) return;
    if (!isNaN(gk) || !isNaN(ck)) {
        const cc = 10; // assume max cc
        const tk = (cc * 1 + (isNaN(gk) ? 0 : gk) * 3 + (isNaN(ck) ? 0 : ck) * 6);
        const xl = tk >= 90 ? 'Xuất sắc' : tk >= 80 ? 'Giỏi' : tk >= 65 ? 'Khá' : tk >= 50 ? 'Trung bình' : 'Không đạt';
        const color = tk >= 80 ? 'success' : tk < 50 ? 'danger' : 'warning';
        el.innerHTML = `Tổng kết dự kiến: <strong class="text-${color}">${tk.toFixed(1)}/100</strong> → <span class="badge bg-${color}">${xl}</span> <small class="text-muted">(với CC=10)</small>`;
    } else {
        el.innerHTML = '';
    }
}

function moCapNhatDiem(id, ten, giuaKy, cuoiKy, nhanXet, daHoanThanh) {
    document.getElementById('diemId').value = id;
    document.getElementById('diemHocVienNameAdmin').textContent = '👤 ' + ten;
    document.getElementById('diemGiuaKy').value = giuaKy ?? '';
    document.getElementById('diemCuoiKy').value = cuoiKy ?? '';
    document.getElementById('nhanXet').value = nhanXet || '';
    document.getElementById('daHoanThanh').checked = daHoanThanh;
    updateDiemPreview();
    new bootstrap.Modal(document.getElementById('modalCapNhatDiem')).show();
}

async function luuDiem() {
    const id = document.getElementById('diemId').value;
    const body = {
        diemGiuaKy: parseFloat(document.getElementById('diemGiuaKy').value) || null,
        diemCuoiKy: parseFloat(document.getElementById('diemCuoiKy').value) || null,
        nhanXet: document.getElementById('nhanXet').value,
        daHoanThanh: document.getElementById('daHoanThanh').checked
    };
    const res = await apiCall(`/api/diem/${id}`, { method:'PUT', body });
    if (res.ok) {
        showToast('success', 'Cập nhật điểm thành công!');
        bootstrap.Modal.getInstance(document.getElementById('modalCapNhatDiem')).hide();
        loadDiemTheoKhoaHoc();
    } else showToast('danger', 'Lỗi cập nhật điểm');
}

// ─── THANH TOÁN ──────────────────────────────────────────────────────
const phuongThucLabel = { 'ChuyenKhoan': '🏦 Chuyển khoản', 'TienMat': '💵 Tiền mặt' };

async function loadThanhToan() {
    const filter = document.getElementById('filterThanhToanStatus')?.value || 'cho';
    const tbody = document.getElementById('tableThanhToan');
    if (!tbody) return;
    tbody.innerHTML = '<tr><td colspan="8" class="text-center py-3"><div class="spinner-border spinner-border-sm"></div></td></tr>';

    try {
        // Load all registrations
        const res = await apiCall('/api/dang-ky');
        const allData = await res.json();

        let data = allData;
        if (filter === 'cho') {
            // Chờ xác nhận: ChoDuyet có phuong thuc + DaDuyet chưa thanh toán
            data = allData.filter(d =>
                (d.trangThai === 'ChoDuyet' && d.phuongThucThanhToan) ||
                (d.trangThai === 'DaDuyet' && !d.ngayThanhToan)
            );
        } else if (filter === 'da-thanh-toan') {
            data = allData.filter(d => d.ngayThanhToan);
        }

        // Badge count
        const chuaXacNhan = allData.filter(d =>
            (d.trangThai === 'ChoDuyet' && d.phuongThucThanhToan) ||
            (d.trangThai === 'DaDuyet' && !d.ngayThanhToan)
        ).length;
        const badge = document.getElementById('badgeThanhToan');
        if (badge) {
            if (chuaXacNhan > 0) { badge.textContent = chuaXacNhan; badge.classList.remove('d-none'); }
            else badge.classList.add('d-none');
        }

        if (!data.length) {
            tbody.innerHTML = '<tr><td colspan="8" class="text-center text-muted py-4">Không có dữ liệu</td></tr>';
            return;
        }

        tbody.innerHTML = data.map(d => {
            const chuaXN = !d.ngayThanhToan && d.phuongThucThanhToan;
            return `<tr class="${chuaXN ? 'table-warning' : ''}">
                <td class="small">
                    <div class="fw-semibold">${d.tenHocVien}</div>
                    <code style="font-size:.7rem">${d.maHocVien}</code>
                </td>
                <td class="small">${d.tenKhoaHoc}</td>
                <td class="small fw-semibold">${formatCurrency(d.hocPhi)}</td>
                <td class="small">
                    ${d.phuongThucThanhToan
                        ? `<span class="badge ${d.phuongThucThanhToan === 'ChuyenKhoan' ? 'bg-primary' : 'bg-success'}">${phuongThucLabel[d.phuongThucThanhToan] || d.phuongThucThanhToan}</span>`
                        : '<span class="text-muted">Chưa chọn</span>'}
                </td>
                <td class="small text-muted">${formatDate(d.ngayDangKy)}</td>
                <td class="small ${d.soTienDaThanhToan > 0 ? 'text-success fw-semibold' : 'text-muted'}">
                    ${d.soTienDaThanhToan > 0 ? formatCurrency(d.soTienDaThanhToan) : '—'}
                    ${d.ngayThanhToan ? `<br><span class="text-muted" style="font-size:.7rem">${formatDate(d.ngayThanhToan)}</span>` : ''}
                </td>
                <td>${trangThaiLabel[d.trangThai] || d.trangThai}</td>
                <td>
                    ${!d.ngayThanhToan && (d.trangThai === 'ChoDuyet' || d.trangThai === 'DaDuyet')
                        ? `<button class="btn btn-success btn-sm" onclick="xacNhanThanhToan(${d.id}, ${d.hocPhi}, '${d.phuongThucThanhToan || ''}')">
                               <i class="bi bi-check-circle me-1"></i>Xác nhận TT
                           </button>`
                        : d.ngayThanhToan
                        ? `<span class="text-success small"><i class="bi bi-check-circle-fill me-1"></i>Đã xác nhận</span>`
                        : '<span class="text-muted small">—</span>'}
                </td>
            </tr>`;
        }).join('');
    } catch(e) {
        tbody.innerHTML = '<tr><td colspan="8" class="text-center text-danger py-3">Lỗi tải dữ liệu. <a href="#" onclick="loadThanhToan()">Thử lại</a></td></tr>';
    }
}

async function xacNhanThanhToan(id, hocPhi, phuongThuc) {
    const soTienInput = prompt(`Xác nhận thanh toán cho đăng ký #${id}\nSố tiền (để trống = học phí đầy đủ: ${formatCurrency(hocPhi)}):`, '');
    if (soTienInput === null) return; // cancelled

    const soTien = soTienInput.trim() === '' ? hocPhi : parseFloat(soTienInput.replace(/[^0-9.]/g, ''));
    if (isNaN(soTien) || soTien <= 0) { showToast('warning', 'Số tiền không hợp lệ'); return; }

    const res = await apiCall(`/api/dang-ky/${id}/thanh-toan`, {
        method: 'PUT',
        body: { soTien, phuongThuc: phuongThuc || null }
    });
    const d = await res.json().catch(() => ({}));
    showToast(res.ok ? 'success' : 'danger', d.message || (res.ok ? '✅ Xác nhận thanh toán thành công!' : 'Lỗi xác nhận'));
    if (res.ok) { loadThanhToan(); loadStats(); }
}

async function xuatBaoCaoDoanhThu() {
    try {
        const res = await apiCall('/api/ai/bao-cao');
        const data = await res.json();

        if (typeof XLSX === 'undefined') { showToast('warning', 'Thư viện Excel chưa tải, vui lòng thử lại'); return; }

        const wb = XLSX.utils.book_new();

        // Sheet 1: Doanh thu theo tháng
        const dtRows = [['Tháng', 'Doanh thu (VND)', 'Số đăng ký']];
        (data.doanhThuTheoThang || []).forEach(r => dtRows.push([r.thang, r.doanhThu, r.soDangKy]));
        const ws1 = XLSX.utils.aoa_to_sheet(dtRows);
        ws1['!cols'] = [{wch:12},{wch:20},{wch:14}];
        XLSX.utils.book_append_sheet(wb, ws1, 'Doanh thu theo tháng');

        // Sheet 2: Học viên mới theo tháng
        const hvRows = [['Tháng', 'Học viên mới']];
        (data.hocVienMoiTheoThang || []).forEach(r => hvRows.push([r.thang, r.soLuong]));
        const ws2 = XLSX.utils.aoa_to_sheet(hvRows);
        ws2['!cols'] = [{wch:12},{wch:16}];
        XLSX.utils.book_append_sheet(wb, ws2, 'Học viên mới');

        // Sheet 3: Ngôn ngữ
        const nnRows = [['Ngôn ngữ', 'Số khóa học']];
        (data.theoNgonNgu || []).forEach(r => nnRows.push([r.ngonNgu, r.soKhoaHoc]));
        const ws3 = XLSX.utils.aoa_to_sheet(nnRows);
        XLSX.utils.book_append_sheet(wb, ws3, 'Ngôn ngữ');

        const fileName = `BaoCaoDoanhThu_${new Date().toISOString().split('T')[0]}.xlsx`;
        XLSX.writeFile(wb, fileName);
        showToast('success', `✅ Đã xuất ${fileName}`);
    } catch(e) {
        showToast('danger', 'Lỗi xuất báo cáo: ' + e.message);
    }
}
