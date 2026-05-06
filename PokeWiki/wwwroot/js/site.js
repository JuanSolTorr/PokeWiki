// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

(function () {
    function buildUrlFromForm(form) {
        var formData = new FormData(form);
        var params = new URLSearchParams();

        for (var pair of formData.entries()) {
            var key = pair[0];
            var value = pair[1];
            var text = (value ?? '').toString();
            if (text !== '') {
                params.set(key, text);
            }
        }

        var action = form.getAttribute('action') || window.location.pathname;
        return params.toString() ? action + '?' + params.toString() : action;
    }

    async function fetchHtml(url, options) {
        var response = await fetch(url, options);
        if (!response.ok) {
            return null;
        }

        return {
            html: await response.text(),
            redirected: response.redirected,
            url: response.url
        };
    }

    function initAjaxListNavigation(config) {
        var container = document.getElementById(config.containerId);
        if (!container) return;

        var ajaxHeaders = { 'X-Requested-With': 'XMLHttpRequest' };

        async function loadContent(url, pushState) {
            try {
                var result = await fetchHtml(url, { headers: ajaxHeaders });
                if (!result) return;

                container.innerHTML = result.html;

                if (pushState) {
                    window.history.pushState({ ajaxList: config.containerId }, '', url);
                }
            } catch {
            }
        }

        container.addEventListener('click', function (event) {
            var link = event.target.closest('a.' + config.linkClass);
            if (!link) return;

            if (event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) return;

            event.preventDefault();
            loadContent(link.href, true);
        });

        container.addEventListener('submit', function (event) {
            var form = event.target.closest('form.' + config.formClass);
            if (!form) return;

            event.preventDefault();
            loadContent(buildUrlFromForm(form), true);
        });

        window.addEventListener('popstate', function () {
            loadContent(window.location.href, false);
        });
    }

    function initAjaxForumForm(config) {
        var form = document.getElementById(config.formId);
        var commentsContainer = document.getElementById(config.commentsContainerId);
        if (!form || !commentsContainer) return;

        form.addEventListener('submit', async function (event) {
            if (typeof $ !== 'undefined' && !$(form).valid()) {
                return;
            }

            event.preventDefault();

            try {
                var result = await fetchHtml(form.action, {
                    method: 'POST',
                    body: new FormData(form),
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                });

                if (!result) return;

                if (result.redirected) {
                    window.location.href = result.url;
                    return;
                }

                commentsContainer.innerHTML = result.html;

                var textArea = form.querySelector('textarea[name="NewMessage"]');
                if (textArea) {
                    textArea.value = '';
                }
            } catch {
            }
        });
    }

    function initMobileMenu() {
        var toggle = document.getElementById('mobile-menu-toggle');
        var menu = document.getElementById('mobile-menu');
        if (!toggle || !menu) return;

        toggle.addEventListener('click', function () {
            var isHidden = menu.classList.toggle('hidden') === false;
            toggle.setAttribute('aria-expanded', isHidden.toString());
        });
    }

    window.PokeWikiAjax = {
        initAjaxListNavigation: initAjaxListNavigation,
        initAjaxForumForm: initAjaxForumForm
    };

    initMobileMenu();
})();
