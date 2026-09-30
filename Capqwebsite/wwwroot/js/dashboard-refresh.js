(() => {
    const container = document.getElementById('dashboard-data');
    if (!container) return;
    const button = document.querySelector('.dashboard .refresh');
    const originalLabel = button.textContent;
    let activeRequest = null;
    let appliedUrl = window.location.href;
    let timer;

    async function update(manual = false) {
        if (!manual && (document.hidden || activeRequest)) return;
        if (manual && activeRequest) activeRequest.abort();
        const request = new AbortController();
        activeRequest = request;
        const timeout = window.setTimeout(() => request.abort(), 15000);
        let url = appliedUrl;
        if (manual) {
            const target = new URL(window.location.href);
            ['year', 'fromDate', 'toDate', 'month', 'productId', 'direction'].forEach(id => {
                const value = document.getElementById(id).value;
                if (value) target.searchParams.set(id, value);
                else target.searchParams.delete(id);
            });
            url = target.href;
            button.textContent = 'جاري التحديث...';
        }
        try {
            const response = await fetch(url, {
                headers: { 'X-Dashboard-Refresh': 'true' },
                cache: 'no-store',
                signal: request.signal
            });
            if (!response.ok || response.redirected) throw new Error('Dashboard refresh failed');
            const documentFragment = new DOMParser().parseFromString(await response.text(), 'text/html');
            const fragment = documentFragment.querySelector('[data-dashboard-fragment]');
            if (!fragment || request.signal.aborted) throw new Error('Invalid dashboard response');
            container.replaceChildren(fragment);
            makeChart('importChart', imports, 10000);
            makeChart('exportChart', exportsData, 5000);
            if (manual) {
                appliedUrl = url;
                window.history.replaceState(null, '', url);
            }
            button.title = 'آخر تحديث: ' + new Date().toLocaleTimeString('ar-EG');
        } catch (error) {
            if (activeRequest === request) button.title = 'تعذر التحديث؛ ستتم إعادة المحاولة تلقائيًا';
        } finally {
            window.clearTimeout(timeout);
            if (activeRequest === request) {
                activeRequest = null;
                button.textContent = originalLabel;
            }
        }
    }

    window.refreshDashboard = () => update(true);
    const schedule = () => {
        window.clearInterval(timer);
        if (!document.hidden) timer = window.setInterval(() => update(), 10000);
        else if (activeRequest) activeRequest.abort();
    };
    document.addEventListener('visibilitychange', schedule);
    window.addEventListener('pagehide', () => {
        window.clearInterval(timer);
        if (activeRequest) activeRequest.abort();
    });
    window.addEventListener('pageshow', schedule);
    schedule();
})();
