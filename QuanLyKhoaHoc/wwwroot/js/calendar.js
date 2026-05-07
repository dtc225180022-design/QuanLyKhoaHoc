// Lịch học – Thời khóa biểu + Danh sách
// Week-aware timetable with current-time line, status badges, Excel/print export

const TIME_SLOTS = [
    '07:00','07:30','08:00','08:30','09:00','09:30','10:00','10:30','11:00','11:30',
    '12:00','12:30','13:00','13:30','14:00','14:30','15:00','15:30','16:00','16:30',
    '17:00','17:30','18:00','18:30','19:00','19:30','20:00','20:30'
];
const THU_NAMES = ['CN', 'Thứ 2', 'Thứ 3', 'Thứ 4', 'Thứ 5', 'Thứ 6', 'Thứ 7'];
const DAY_SHORT = ['CN', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7'];
// Map: DayOfWeek (0=CN,1=T2,...,6=T7) → column index (0=T2,...,5=T7,6=CN)
const DAY_COL = { 1:0, 2:1, 3:2, 4:3, 5:4, 6:5, 0:6 };
// Reverse: column → DayOfWeek
const COL_DAY = [1, 2, 3, 4, 5, 6, 0];

let currentView = 'timetable';
let lichData = [];
let filterMode = 'all';
let weekOffset = 0; // 0 = current week

// ─── Week helpers ───────────────────────────────────────────────────
function getMondayOfWeek(offset) {
    const now = new Date();
    const day = now.getDay(); // 0=CN
    const diffToMon = day === 0 ? -6 : 1 - day;
    const mon = new Date(now);
    mon.setDate(now.getDate() + diffToMon + offset * 7);
    mon.setHours(0,0,0,0);
    return mon;
}

function getWeekDates(offset) {
    const mon = getMondayOfWeek(offset);
    return Array.from({length:7}, (_, i) => {
        const d = new Date(mon);
        d.setDate(mon.getDate() + i); // Mon+0..Mon+6 = T2..CN
        return d;
    });
}

function fmtDate(d) {
    return d.getDate() + '/' + (d.getMonth()+1);
}

function changeWeek(delta) {
    if (delta === 0) weekOffset = 0;
    else weekOffset += delta;
    renderView();
    updateWeekHeaders();
}

function updateWeekHeaders() {
    const dates = getWeekDates(weekOffset);
    // Column order: T2,T3,T4,T5,T6,T7,CN → dates[0..6]
    const today = new Date();
    today.setHours(0,0,0,0);
    const dayIds = [1,2,3,4,5,6,0]; // DayOfWeek values for th-1..th-0
    dates.forEach((d, i) => {
        const dow = COL_DAY[i]; // DayOfWeek
        const th = document.getElementById('th-' + dow);
        if (!th) return;
        const isToday = d.getTime() === today.getTime();
        th.innerHTML = `<div class="fw-semibold">${DAY_SHORT[dow]}</div><div style="font-size:.75rem;opacity:.8">${fmtDate(d)}</div>`;
        th.style.background = isToday ? '#0a58ca' : '';
    });

    // Update subtitle with week range
    const sub = document.getElementById('subtitleLich');
    if (sub) {
        const mon = dates[0], sun = dates[6];
        let weekLabel = weekOffset === 0 ? 'Tuần này' : weekOffset === 1 ? 'Tuần sau' : weekOffset === -1 ? 'Tuần trước' : `Tuần +${weekOffset}`;
        sub.textContent = `${weekLabel}: ${fmtDate(mon)} – ${fmtDate(sun)}/${sun.getFullYear()}`;
    }
}

// ─── Status helpers ─────────────────────────────────────────────────
function getDayDate(dowTarget, offset) {
    const dates = getWeekDates(offset);
    const colIdx = DAY_COL[dowTarget];
    return colIdx !== undefined ? dates[colIdx] : null;
}

function getTrangThaiForDate(l, date) {
    if (!date) return null;
    const today = new Date(); today.setHours(0,0,0,0);
    const isToday = date.getTime() === today.getTime();
    if (!isToday) return null;

    const now = new Date();
    const [h0, m0] = l.gioBatDau.split(':').map(Number);
    const [h1, m1] = l.gioKetThuc.split(':').map(Number);
    const startMin = h0*60+m0, endMin = h1*60+m1;
    const nowMin = now.getHours()*60+now.getMinutes();

    if (nowMin < startMin) return 'sap-toi';
    if (nowMin >= startMin && nowMin < endMin) return 'dang-dien-ra';
    return 'da-qua';
}

// Legacy compat for list view
function getTrangThaiHomNay(l) {
    const today = new Date();
    if (today.getDay() !== l.thu) return null;
    const now = new Date();
    const [h0,m0] = l.gioBatDau.split(':').map(Number);
    const [h1,m1] = l.gioKetThuc.split(':').map(Number);
    const startMin = h0*60+m0, endMin = h1*60+m1;
    const nowMin = now.getHours()*60+now.getMinutes();
    if (nowMin < startMin) return 'sap-toi';
    if (nowMin >= startMin && nowMin < endMin) return 'dang-dien-ra';
    return 'da-qua';
}

function getNextDate(thu) {
    const today = new Date();
    const todayDow = today.getDay();
    let diff = thu - todayDow;
    if (diff < 0) diff += 7;
    if (diff === 0) diff = 7;
    const d = new Date(today);
    d.setDate(d.getDate() + diff);
    return d;
}

// ─── Main load ──────────────────────────────────────────────────────
function switchView(view) {
    currentView = view;
    document.getElementById('btnTimetable').classList.toggle('active', view === 'timetable');
    document.getElementById('btnList').classList.toggle('active', view === 'list');
    renderView();
}

function setFilter(mode, btn) {
    filterMode = mode;
    document.querySelectorAll('.filter-btn').forEach(b => {
        b.className = b.className.replace('btn-primary','btn-outline-primary');
    });
    if (btn) btn.className = btn.className.replace('btn-outline-primary','btn-primary');
    renderView();
}

async function loadMyLich() {
    document.getElementById('loadingLich').style.display = '';
    document.getElementById('viewTimetable').style.display = 'none';
    document.getElementById('viewList').style.display = 'none';
    document.getElementById('emptyState').style.display = 'none';

    try {
        const token = localStorage.getItem('jwt_token');
        const url = token ? '/api/lich-hoc/cua-toi' : '/api/lich-hoc';
        const res = await (token ? apiCall(url) : fetch(url));
        lichData = await res.json();
    } catch(e) { lichData = []; }

    document.getElementById('loadingLich').style.display = 'none';
    updateWeekHeaders();
    renderView();

    // Auto-scroll to current time after render
    if (currentView === 'timetable') {
        setTimeout(scrollToCurrentTime, 300);
    }
}

function renderView() {
    if (!lichData.length) {
        document.getElementById('emptyState').style.display = '';
        document.getElementById('viewTimetable').style.display = 'none';
        document.getElementById('viewList').style.display = 'none';
        return;
    }
    document.getElementById('emptyState').style.display = 'none';
    if (currentView === 'timetable') {
        renderTimetable();
        document.getElementById('viewTimetable').style.display = '';
        document.getElementById('viewList').style.display = 'none';
        setTimeout(renderCurrentTimeLine, 100);
    } else {
        renderList();
        document.getElementById('viewList').style.display = '';
        document.getElementById('viewTimetable').style.display = 'none';
    }
}

// ─── Timetable ──────────────────────────────────────────────────────
function renderTimetable() {
    const cellMap = {};
    for (let i=0; i<7; i++) cellMap[i] = [];
    lichData.forEach(l => {
        const col = DAY_COL[l.thu];
        if (col !== undefined) cellMap[col].push(l);
    });

    const allSlots = lichData.flatMap(l => [l.gioBatDau, l.gioKetThuc]);
    if (!allSlots.length) { document.getElementById('timetableBody').innerHTML = ''; return; }

    const minT = allSlots.reduce((a,b) => a < b ? a : b);
    const maxT = allSlots.reduce((a,b) => a > b ? a : b);
    const slots = TIME_SLOTS.filter(t => t >= minT && t <= maxT);
    if (!slots.length) return;

    const dates = getWeekDates(weekOffset);
    const today = new Date(); today.setHours(0,0,0,0);

    const tbody = document.getElementById('timetableBody');
    tbody.innerHTML = slots.map((slot, slotIdx) => {
        const cells = Array.from({length:7}, (_, col) => {
            const colDate = dates[col];
            const isToday = colDate.getTime() === today.getTime();
            const lessons = cellMap[col].filter(l => l.gioBatDau <= slot && l.gioKetThuc > slot);
            const bgStyle = isToday ? 'background:#fffef0;' : '';

            if (!lessons.length) {
                return `<td style="min-width:130px;${bgStyle}padding:4px" data-slot="${slot}" data-col="${col}"></td>`;
            }

            return lessons.map(l => {
                const trangThai = getTrangThaiForDate(l, isToday ? new Date() : null);
                let statusBadge = '';
                if (trangThai === 'dang-dien-ra') {
                    statusBadge = `<span class="badge bg-danger d-inline-flex align-items-center gap-1" style="font-size:.6rem">
                        <span class="rounded-circle bg-white" style="width:6px;height:6px;animation:pulse 1s infinite;display:inline-block"></span>LIVE
                    </span>`;
                } else if (trangThai === 'sap-toi') {
                    statusBadge = `<span class="badge bg-success" style="font-size:.6rem">Sắp tới</span>`;
                } else if (trangThai === 'da-qua') {
                    statusBadge = `<span class="badge bg-secondary" style="font-size:.6rem">Đã qua</span>`;
                }
                const meetBtn = (l.linkMeet && l.hinhThuc !== 'Offline')
                    ? `<a href="${l.linkMeet}" target="_blank" class="text-decoration-none d-inline-flex align-items-center gap-1 mt-1" style="font-size:.65rem;color:#0d6efd">
                           <i class="bi bi-camera-video-fill"></i>Meet
                       </a>`
                    : '';
                const opacity = trangThai === 'da-qua' ? 'opacity:.6;' : '';
                return `
                <td style="min-width:140px;${bgStyle}${opacity}padding:4px;vertical-align:top" data-slot="${slot}" data-col="${col}">
                    <div class="lich-cell${trangThai === 'dang-dien-ra' ? ' lich-cell-live' : ''}" style="
                        background:${l.mauSac}18;
                        border-left:3px solid ${l.mauSac};
                        border-radius:8px;
                        padding:7px 9px;
                        height:100%;
                        min-height:64px">
                        <div class="d-flex justify-content-between align-items-start gap-1 mb-1">
                            <div class="fw-semibold lh-sm" style="font-size:.8rem;color:${l.mauSac}">${l.tenKhoaHoc}</div>
                            ${statusBadge}
                        </div>
                        <div style="font-size:.7rem;color:#666"><i class="bi bi-clock me-1"></i>${l.gioBatDau}–${l.gioKetThuc}</div>
                        <div style="font-size:.7rem;color:#666"><i class="bi bi-door-open me-1"></i>${l.phongHoc}</div>
                        <div style="font-size:.7rem;color:#666"><i class="bi bi-person me-1"></i>${l.giangVien}</div>
                        ${meetBtn ? `<div>${meetBtn}</div>` : ''}
                    </div>
                </td>`;
            }).join('');
        });

        return `<tr data-slot="${slot}">
            <td class="text-center text-muted fw-semibold align-middle" style="font-size:.8rem;background:#f8f9fa;white-space:nowrap;padding:6px 10px">${slot}</td>
            ${cells.join('')}
        </tr>`;
    }).join('');
}

// ─── Current-time line ──────────────────────────────────────────────
function renderCurrentTimeLine() {
    // Remove old line
    document.querySelectorAll('.current-time-line-row').forEach(el => el.remove());
    if (weekOffset !== 0) return; // Only show for current week

    const now = new Date();
    const todayDow = now.getDay();
    const nowMin = now.getHours()*60 + now.getMinutes();

    // Find which slot row to add the line to
    const tbody = document.getElementById('timetableBody');
    if (!tbody) return;

    const rows = tbody.querySelectorAll('tr');
    let targetRow = null, prevMin = -1;

    rows.forEach(row => {
        const slot = row.getAttribute('data-slot');
        if (!slot) return;
        const [h,m] = slot.split(':').map(Number);
        const slotMin = h*60+m;
        if (slotMin <= nowMin) { targetRow = row; prevMin = slotMin; }
    });

    if (!targetRow || prevMin < 0) return;

    // Add a line overlay inside the today column td
    const col = DAY_COL[todayDow];
    if (col === undefined) return;
    const tds = targetRow.querySelectorAll('td');
    // tds[0] = time label, tds[1..7] = day columns
    const targetTd = tds[col + 1];
    if (!targetTd) return;

    // Calculate position within slot (30-min slot = 100%)
    const slotDuration = 30;
    const pct = Math.min(100, Math.round(((nowMin - prevMin) / slotDuration) * 100));

    const line = document.createElement('div');
    line.className = 'current-time-line-row';
    line.style.cssText = `position:absolute;left:0;right:0;top:${pct}%;height:2px;background:#dc3545;z-index:20;pointer-events:none`;
    line.innerHTML = `<div style="position:absolute;left:-4px;top:-4px;width:10px;height:10px;border-radius:50%;background:#dc3545"></div>`;

    if (getComputedStyle(targetTd).position === 'static') {
        targetTd.style.position = 'relative';
    }
    targetTd.appendChild(line);
}

function scrollToCurrentTime() {
    const now = new Date();
    const nowStr = `${String(now.getHours()).padStart(2,'0')}:${String(Math.floor(now.getMinutes()/30)*30).padStart(2,'0')}`;
    const row = document.querySelector(`#timetableBody tr[data-slot="${nowStr}"]`);
    if (row) row.scrollIntoView({ behavior: 'smooth', block: 'center' });
}

// ─── List view ──────────────────────────────────────────────────────
function renderList() {
    const now = new Date();
    const todayDow = now.getDay();
    const todayName = THU_NAMES[todayDow];

    let filtered = lichData;
    if (filterMode === 'today') {
        filtered = lichData.filter(l => l.thu === todayDow);
    } else if (filterMode === 'week') {
        const nextWeek = new Set();
        for (let i=0; i<7; i++) {
            const d = new Date(now); d.setDate(d.getDate() + i);
            nextWeek.add(d.getDay());
        }
        filtered = lichData.filter(l => nextWeek.has(l.thu));
    }

    const container = document.getElementById('listContainer');
    if (!filtered.length) {
        container.innerHTML = `<div class="col-12 text-center text-muted py-5">
            <i class="bi bi-calendar-x fs-1 d-block mb-2"></i>
            <p>Không có lịch học ${filterMode === 'today' ? 'hôm nay' : 'trong tuần này'}</p>
        </div>`;
        return;
    }

    const grouped = {};
    filtered.forEach(l => {
        const key = THU_NAMES[l.thu] || `Thứ ${l.thu}`;
        if (!grouped[key]) grouped[key] = [];
        grouped[key].push(l);
    });

    const thuOrder = ['Thứ 2','Thứ 3','Thứ 4','Thứ 5','Thứ 6','Thứ 7','CN'];
    container.innerHTML = thuOrder
        .filter(thu => grouped[thu])
        .map(thu => {
            const isToday = thu === todayName;
            const thuItems = grouped[thu].sort((a,b) => a.gioBatDau.localeCompare(b.gioBatDau));
            return `
                <div class="col-md-6 col-lg-4">
                    <div class="card border-0 shadow-sm" style="${isToday ? 'border:2px solid #ffc107!important' : ''}">
                        <div class="card-header ${isToday ? 'bg-warning' : 'bg-light'} border-0 py-2 px-3">
                            <h6 class="mb-0 fw-bold">
                                ${isToday ? '📅 ' : ''}${thu}
                                ${isToday ? '<span class="badge bg-danger ms-2" style="font-size:.65rem">Hôm nay</span>' : ''}
                            </h6>
                        </div>
                        <div class="card-body p-2">
                            ${thuItems.map(l => renderListItem(l, isToday)).join('')}
                        </div>
                    </div>
                </div>`;
        }).join('');
}

function renderListItem(l, isToday) {
    const trangThai = isToday ? getTrangThaiHomNay(l) : null;
    let statusHtml = '';
    if (trangThai === 'dang-dien-ra') {
        statusHtml = `<span class="badge bg-danger ms-1 d-inline-flex align-items-center gap-1" style="font-size:.62rem">
            <span class="rounded-circle bg-white" style="width:6px;height:6px;animation:pulse 1s infinite;display:inline-block"></span>Đang diễn ra
        </span>`;
    } else if (trangThai === 'sap-toi') {
        const [h,m] = l.gioBatDau.split(':').map(Number);
        const now = new Date();
        const diff = h*60+m - (now.getHours()*60+now.getMinutes());
        const label = diff < 60 ? `${diff} phút nữa` : `${Math.round(diff/60)} giờ nữa`;
        statusHtml = `<span class="badge bg-success ms-1" style="font-size:.62rem">⏰ ${label}</span>`;
    } else if (trangThai === 'da-qua') {
        statusHtml = `<span class="badge bg-secondary ms-1" style="font-size:.62rem">✓ Đã qua</span>`;
    }

    const hinhThucIcon = l.hinhThuc === 'Online' ? '💻' : l.hinhThuc === 'KetHop' ? '🔀' : '🏫';
    const meetBtn = (l.linkMeet && l.hinhThuc !== 'Offline')
        ? `<a href="${l.linkMeet}" target="_blank" class="btn btn-sm btn-outline-primary py-0 mt-1" style="font-size:.7rem">
               <i class="bi bi-camera-video-fill me-1"></i>Tham gia Meet
           </a>` : '';

    return `
        <div class="p-2 rounded mb-2" style="background:${l.mauSac}12;border-left:3px solid ${l.mauSac}${trangThai === 'dang-dien-ra' ? ';box-shadow:0 0 0 2px '+l.mauSac+'44' : ''};${trangThai === 'da-qua' ? 'opacity:.65' : ''}">
            <div class="d-flex align-items-start gap-2">
                <div class="text-center" style="min-width:54px">
                    <div class="fw-bold small" style="color:${l.mauSac}">${l.gioBatDau}</div>
                    <div class="text-muted" style="font-size:.65rem">↓ ${l.gioKetThuc}</div>
                </div>
                <div class="flex-fill">
                    <div class="fw-semibold small d-flex align-items-center flex-wrap gap-1" style="color:${l.mauSac}">
                        ${l.tenKhoaHoc}${statusHtml}
                    </div>
                    <div class="d-flex flex-wrap gap-2 mt-1">
                        <span class="text-muted" style="font-size:.68rem"><i class="bi bi-person me-1"></i>${l.giangVien}</span>
                        <span class="text-muted" style="font-size:.68rem">${hinhThucIcon} ${l.hinhThuc === 'Offline' ? l.phongHoc : l.hinhThuc}</span>
                        <span class="badge text-white" style="background:${l.mauSac};font-size:.6rem">${l.ngonNgu||''}</span>
                    </div>
                    ${meetBtn}
                </div>
            </div>
        </div>`;
}

// ─── Export ─────────────────────────────────────────────────────────
function taiLichHoc() {
    if (!lichData.length) { alert('Không có lịch học để tải'); return; }

    // Build CSV with all schedule data
    const thuTenMap = {0:'CN',1:'Thứ 2',2:'Thứ 3',3:'Thứ 4',4:'Thứ 5',5:'Thứ 6',6:'Thứ 7'};
    const rows = [['Thứ', 'Giờ bắt đầu', 'Giờ kết thúc', 'Khóa học', 'Ngôn ngữ', 'Giảng viên', 'Phòng học', 'Hình thức', 'Link Meet']];
    lichData
        .sort((a,b) => (DAY_COL[a.thu]??7) - (DAY_COL[b.thu]??7) || a.gioBatDau.localeCompare(b.gioBatDau))
        .forEach(l => {
            rows.push([
                thuTenMap[l.thu] || l.thu,
                l.gioBatDau,
                l.gioKetThuc,
                l.tenKhoaHoc,
                l.ngonNgu || '',
                l.giangVien || '',
                l.phongHoc || '',
                l.hinhThuc || '',
                l.linkMeet || ''
            ]);
        });

    // Check if SheetJS (XLSX) is available
    if (typeof XLSX !== 'undefined') {
        const wb = XLSX.utils.book_new();
        const ws = XLSX.utils.aoa_to_sheet(rows);
        ws['!cols'] = [{wch:10},{wch:12},{wch:12},{wch:30},{wch:12},{wch:20},{wch:12},{wch:12},{wch:35}];
        XLSX.utils.book_append_sheet(wb, ws, 'Lịch học');
        XLSX.writeFile(wb, `lich_hoc_${new Date().toISOString().slice(0,10)}.xlsx`);
    } else {
        // Fallback: CSV
        const csv = rows.map(r => r.map(c => `"${String(c).replace(/"/g,'""')}"`).join(',')).join('\n');
        const blob = new Blob(['﻿'+csv], {type:'text/csv;charset=utf-8;'});
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url; a.download = `lich_hoc_${new Date().toISOString().slice(0,10)}.csv`; a.click();
        URL.revokeObjectURL(url);
    }
}
