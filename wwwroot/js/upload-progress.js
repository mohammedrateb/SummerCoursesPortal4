/* ════════════════════════════════════════════════════════════════
   upload-progress.js
   --------------------------------------------------------------
   بيحوّل أى <form enctype="multipart/form-data"> من submit عادى
   إلى رفع AJAX (XMLHttpRequest) مع شريط تقدّم وOverlay،
   علشان المستخدم يشوف إن الرفع شغّال ومايفتكرش إن الصفحة "علّقت".

   الاستخدام:
       window.UploadProgress.attach(formElement, {
           submitBtn:  buttonElement,   // اختيارى
           onBeforeSend: (formData) => {}, // اختيارى - عدّل البيانات قبل الإرسال
           successText: 'تم الحفظ بنجاح'
       });
   ════════════════════════════════════════════════════════════════ */
(function () {
    'use strict';

    // أنشئ الـ overlay مرة واحدة وضيفه على body عند الحاجة
    function buildOverlay() {
        if (document.getElementById('upx-overlay')) return;

        const style = document.createElement('style');
        style.textContent = `
            #upx-overlay {
                position: fixed; inset: 0;
                background: rgba(15, 23, 42, .78);
                backdrop-filter: blur(4px);
                display: none;
                align-items: center; justify-content: center;
                z-index: 99999;
                font-family: 'Cairo', 'Tajawal', system-ui, sans-serif;
                direction: rtl;
            }
            #upx-overlay.is-active { display: flex; }
            .upx-card {
                background: #fff; color: #0f172a;
                border-radius: 18px;
                padding: 28px 32px;
                width: min(440px, 92vw);
                box-shadow: 0 25px 60px -12px rgba(0,0,0,.45);
                text-align: center;
            }
            .upx-icon {
                width: 56px; height: 56px;
                margin: 0 auto 14px;
                border-radius: 50%;
                background: linear-gradient(135deg, #6366f1, #8b5cf6);
                display: flex; align-items: center; justify-content: center;
                color: #fff; font-size: 24px;
                animation: upx-pulse 1.6s ease-in-out infinite;
            }
            @keyframes upx-pulse {
                0%,100% { transform: scale(1); box-shadow: 0 0 0 0 rgba(139,92,246,.55); }
                50%     { transform: scale(1.06); box-shadow: 0 0 0 14px rgba(139,92,246,0); }
            }
            .upx-title { font-size: 18px; font-weight: 700; margin: 0 0 6px; }
            .upx-sub   { font-size: 13px; color: #64748b; margin: 0 0 18px; }
            .upx-bar-wrap {
                background: #e2e8f0;
                border-radius: 999px;
                height: 12px;
                overflow: hidden;
                margin-bottom: 10px;
            }
            .upx-bar {
                height: 100%;
                width: 0%;
                background: linear-gradient(90deg, #6366f1, #8b5cf6, #ec4899);
                background-size: 200% 100%;
                animation: upx-shimmer 2s linear infinite;
                border-radius: 999px;
                transition: width .25s ease-out;
            }
            @keyframes upx-shimmer {
                0%   { background-position: 0% 50%; }
                100% { background-position: 200% 50%; }
            }
            .upx-stats {
                display: flex; justify-content: space-between;
                font-size: 12px; color: #475569;
                font-variant-numeric: tabular-nums;
            }
            .upx-card.is-success .upx-icon {
                background: linear-gradient(135deg, #10b981, #059669);
                animation: none;
            }
            .upx-card.is-error .upx-icon {
                background: linear-gradient(135deg, #ef4444, #dc2626);
                animation: none;
            }
        `;
        document.head.appendChild(style);

        const overlay = document.createElement('div');
        overlay.id = 'upx-overlay';
        overlay.innerHTML = `
            <div class="upx-card" id="upx-card">
                <div class="upx-icon" id="upx-icon">
                    <i class="fas fa-cloud-upload-alt"></i>
                </div>
                <h3 class="upx-title" id="upx-title">جارٍ رفع المنشور…</h3>
                <p class="upx-sub" id="upx-sub">من فضلك ماتقفلش الصفحة.</p>
                <div class="upx-bar-wrap"><div class="upx-bar" id="upx-bar"></div></div>
                <div class="upx-stats">
                    <span id="upx-percent">0%</span>
                    <span id="upx-size">0 / 0 MB</span>
                </div>
            </div>
        `;
        document.body.appendChild(overlay);
    }

    function fmtMB(bytes) {
        return (bytes / 1048576).toFixed(1) + ' MB';
    }

    function show()       { document.getElementById('upx-overlay').classList.add('is-active'); }
    function setPercent(p, loaded, total) {
        document.getElementById('upx-bar').style.width    = p + '%';
        document.getElementById('upx-percent').textContent = p + '%';
        if (total) {
            document.getElementById('upx-size').textContent = fmtMB(loaded) + ' / ' + fmtMB(total);
        }
    }
    function setState(state, title, sub) {
        const card = document.getElementById('upx-card');
        card.classList.remove('is-success', 'is-error');
        if (state) card.classList.add('is-' + state);

        const icon = document.getElementById('upx-icon');
        icon.innerHTML = state === 'success'
            ? '<i class="fas fa-check"></i>'
            : state === 'error'
                ? '<i class="fas fa-exclamation-triangle"></i>'
                : '<i class="fas fa-cloud-upload-alt"></i>';

        if (title) document.getElementById('upx-title').textContent = title;
        if (sub  !== undefined) document.getElementById('upx-sub').textContent = sub;
    }

    function attach(form, opts) {
        if (!form) return;
        opts = opts || {};
        buildOverlay();

        form.addEventListener('submit', function (e) {
            // لو حد لغى الـ submit (مثلاً الفورم disabled) ما نعملش حاجة
            if (e.defaultPrevented) return;
            e.preventDefault();

            // عطّل زر الـ submit لمنع الضغط المتكرر
            if (opts.submitBtn) {
                opts.submitBtn.disabled = true;
                opts.submitBtn.classList.add('is-loading');
            }

            const fd = new FormData(form);
            if (typeof opts.onBeforeSend === 'function') {
                try { opts.onBeforeSend(fd, form); } catch (_) { }
            }

            show();
            setState(null, 'جارٍ رفع المنشور…', 'من فضلك ماتقفلش الصفحة.');
            setPercent(0, 0, 0);

            const xhr = new XMLHttpRequest();
            xhr.open(form.method || 'POST', form.action, true);
            // مهم: علشان نعرف لو الـ controller عمل redirect
            xhr.responseType = 'text';

            xhr.upload.addEventListener('progress', function (ev) {
                if (!ev.lengthComputable) return;
                const pct = Math.round((ev.loaded / ev.total) * 100);
                setPercent(pct, ev.loaded, ev.total);
                if (pct >= 100) {
                    setState(null, 'جارٍ معالجة المنشور…', 'الرفع اكتمل، السيرفر بيحفظ البيانات.');
                }
            });

            xhr.addEventListener('load', function () {
                // Redirect ناجح بيرجع 200 + responseURL مختلف عن action
                const isSuccess = xhr.status >= 200 && xhr.status < 400;
                const wasRedirected = xhr.responseURL &&
                    xhr.responseURL.replace(/\/$/, '') !==
                    new URL(form.action, window.location.href).href.replace(/\/$/, '');

                if (isSuccess && wasRedirected) {
                    setState('success', opts.successText || 'تم الحفظ بنجاح ✓', 'جارٍ تحويلك…');
                    setPercent(100, 0, 0);
                    setTimeout(function () {
                        window.location.href = xhr.responseURL;
                    }, 600);
                } else if (isSuccess) {
                    // الـ controller رجّع الـ View تانى (validation errors)
                    // نستبدل الصفحة الحالية بالـ HTML الراجع
                    document.open();
                    document.write(xhr.responseText);
                    document.close();
                } else {
                    setState('error',
                        'فشل رفع المنشور',
                        'كود الخطأ: ' + xhr.status + '. حاول تانى أو راجع حجم الملف.');
                    if (opts.submitBtn) {
                        opts.submitBtn.disabled = false;
                        opts.submitBtn.classList.remove('is-loading');
                    }
                }
            });

            xhr.addEventListener('error', function () {
                setState('error',
                    'فشل الاتصال بالسيرفر',
                    'تأكد إن السيرفر شغّال وحاول تانى.');
                if (opts.submitBtn) {
                    opts.submitBtn.disabled = false;
                    opts.submitBtn.classList.remove('is-loading');
                }
            });

            xhr.addEventListener('abort', function () {
                setState('error', 'تم إلغاء الرفع', '');
                if (opts.submitBtn) {
                    opts.submitBtn.disabled = false;
                    opts.submitBtn.classList.remove('is-loading');
                }
            });

            xhr.send(fd);
        });
    }

    window.UploadProgress = { attach: attach };
})();
