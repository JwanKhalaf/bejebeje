// ga4 sign_up_completed event — best effort, fires on first page after new signup + login
(function () {
  'use strict';

  // guard: bail if gtag is not available
  if (typeof gtag === 'undefined') return;
  if (typeof window.BJ_ANALYTICS === 'undefined') return;

  var ctx = document.getElementById('signup-completed-context');
  if (!ctx) return;

  // attempt to read attribution from url params if available
  var params = new URLSearchParams(window.location.search);
  var entryPoint = params.get('entry_point') || '';
  var sourceSection = params.get('source_section') || '';

  gtag('event', window.BJ_ANALYTICS.EVENTS.SIGN_UP_COMPLETED, {
    source_page: window.location.pathname === '/' ? 'home' : '',
    source_section: sourceSection,
    ui_variant: 'homepage_v1',
    entry_point: entryPoint,
    post_auth_destination: window.location.pathname,
    auth_provider: 'cognito',
  });
})();
