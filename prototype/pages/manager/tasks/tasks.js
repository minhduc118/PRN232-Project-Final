/* ============================================================
   COURTMANAGER PRO — TASK MANAGEMENT JS
   pages/manager/tasks/tasks.js
   ============================================================ */

document.addEventListener('DOMContentLoaded', function() {
    
    // ── Elements Selection ──
    const kanbanBtn = document.getElementById('kanban-btn');
    const listBtn = document.getElementById('list-btn');
    const kanbanView = document.getElementById('kanban-view');
    const listView = document.getElementById('list-view');
    const sidebar = document.getElementById('creation-sidebar');
    const fabBtn = document.getElementById('fabBtn');
    const closeSidebarBtn = document.getElementById('closeSidebarBtn');
    const createTaskBtn = document.getElementById('createTaskBtn');

    // ── Toggle Kanban vs List View ──
    if (kanbanBtn && listBtn && kanbanView && listView) {
        kanbanBtn.addEventListener('click', () => {
            kanbanBtn.classList.add('bg-primary', 'text-on-primary-container');
            kanbanBtn.classList.remove('text-on-surface-variant');
            listBtn.classList.remove('bg-primary', 'text-on-primary-container');
            listBtn.classList.add('text-on-surface-variant');
            
            kanbanView.classList.remove('hidden');
            listView.classList.add('hidden');
        });

        listBtn.addEventListener('click', () => {
            listBtn.classList.add('bg-primary', 'text-on-primary-container');
            listBtn.classList.remove('text-on-surface-variant');
            kanbanBtn.classList.remove('bg-primary', 'text-on-primary-container');
            kanbanBtn.classList.add('text-on-surface-variant');
            
            listView.classList.remove('hidden');
            kanbanView.classList.add('hidden');
        });
    }

    // ── Sidebar Toggle Helpers ──
    function toggleSidebar() {
        if (sidebar) {
            sidebar.classList.toggle('translate-x-full');
        }
    }

    if (fabBtn) fabBtn.addEventListener('click', toggleSidebar);
    if (closeSidebarBtn) closeSidebarBtn.addEventListener('click', toggleSidebar);
    if (createTaskBtn) createTaskBtn.addEventListener('click', toggleSidebar);

    // ── Drag & Hover Card Scaling Feedback ──
    const initCardInteractions = (card) => {
        card.addEventListener('mousedown', () => {
            card.style.transform = 'scale(0.97)';
            card.style.boxShadow = '0 0 15px rgba(192, 193, 255, 0.1)';
        });
        card.addEventListener('mouseup', () => {
            card.style.transform = 'scale(1)';
            card.style.boxShadow = 'none';
        });
    };
    document.querySelectorAll('.glass-card').forEach(initCardInteractions);

    // ── Dynamic Mouse-tracking Radial Glow micro-interactions ──
    const initRadialGlow = (element) => {
        element.addEventListener('mousemove', (e) => {
            const rect = element.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const y = e.clientY - rect.top;
            element.style.setProperty('--mouse-x', `${x}px`);
            element.style.setProperty('--mouse-y', `${y}px`);
        });
    };
    document.querySelectorAll('.glass-card, .glass-panel').forEach(initRadialGlow);

    // ── Form Input & Dynamic Task Creation (Kanban Integration) ──
    const form = sidebar ? sidebar.querySelector('form') : null;
    const generateBtn = form ? form.querySelector('button[type="button"]') : null;
    const backlogColumn = kanbanView ? kanbanView.querySelector('.kanban-column') : null;

    if (generateBtn && form && backlogColumn) {
        generateBtn.addEventListener('click', function() {
            const titleInput = form.querySelector('input[type="text"]');
            const prioritySelect = form.querySelectorAll('select')[0];
            const assigneeSelect = form.querySelectorAll('select')[1];

            if (!titleInput || titleInput.value.trim() === '') {
                alert('Vui lòng nhập tiêu đề cho công việc mới!');
                return;
            }

            const title = titleInput.value.trim();
            const priority = prioritySelect ? prioritySelect.value : 'Medium';
            const assignee = assigneeSelect ? assigneeSelect.value : 'Mike R.';

            // Create new Kanban card element
            const newCard = document.createElement('div');
            newCard.className = 'glass-card p-4 rounded-xl flex flex-col gap-3 group cursor-pointer relative overflow-hidden transition-all duration-300 transform scale-90 opacity-0';

            // Set priority colors dynamically
            let priorityBadgeColor = 'bg-surface-variant text-on-surface-variant';
            if (priority === 'Urgent') priorityBadgeColor = 'bg-error-container/20 text-error';
            else if (priority === 'High') priorityBadgeColor = 'bg-tertiary-container/20 text-tertiary';
            else if (priority === 'Medium') priorityBadgeColor = 'bg-primary-container/20 text-primary';

            newCard.innerHTML = `
                <div class="flex justify-between items-start">
                    <span class="text-[10px] bg-surface-variant text-on-surface-variant px-2 py-0.5 rounded-full uppercase font-bold tracking-tight">Manual</span>
                    <span class="text-[10px] ${priorityBadgeColor} px-2 py-0.5 rounded-full uppercase font-bold">${priority}</span>
                </div>
                <h5 class="font-body-md text-body-md text-on-surface leading-snug group-hover:text-primary transition-colors">${title}</h5>
                <div class="flex items-center justify-between mt-2 pt-3 border-t border-outline-variant/20">
                    <div class="flex items-center gap-2 text-on-surface-variant">
                        <span class="material-symbols-outlined text-sm">schedule</span>
                        <span class="text-[11px]">Due Today</span>
                    </div>
                    <div class="w-6 h-6 rounded-full bg-primary/20 flex items-center justify-center font-bold text-[8px] text-primary border border-outline-variant">
                        ${assignee.split(' ').map(n => n[0]).join('')}
                    </div>
                </div>
            `;

            // Append new card to backlog column
            backlogColumn.appendChild(newCard);
            
            // Set radial glow and drag interactions on the new card
            initRadialGlow(newCard);
            initCardInteractions(newCard);

            // Animate card entry
            setTimeout(() => {
                newCard.classList.remove('scale-90', 'opacity-0');
                newCard.classList.add('scale-100', 'opacity-100');
            }, 50);

            // Reset inputs & close sidebar
            titleInput.value = '';
            toggleSidebar();
        });
    }
});
