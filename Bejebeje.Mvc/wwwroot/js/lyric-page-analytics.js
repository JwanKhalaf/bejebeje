// ga4 lyric_page_cta_clicked event
(function () {
  'use strict';

  // guard: bail if gtag is not available
  if (typeof gtag === 'undefined') return;
  if (typeof window.BJ_ANALYTICS === 'undefined') return;

  var cta = document.getElementById('lyric-report-cta');
  if (!cta) return;

  cta.addEventListener('click', function () {
    var lyricId = parseInt(cta.getAttribute('data-lyric-id'), 10);
    var isAuthenticated = cta.getAttribute('data-is-authenticated') === 'true';

    // use beacon transport since we're navigating away
    gtag('event', window.BJ_ANALYTICS.EVENTS.LYRIC_PAGE_CTA_CLICKED, {
      source_page: 'lyric_detail',
      source_section: window.BJ_ANALYTICS.SOURCE_SECTIONS.SIDE_ACTIONS,
      is_authenticated: isAuthenticated,
      cta_name: window.BJ_ANALYTICS.CTA_NAMES.REPORT_ISSUE,
      entry_point: window.BJ_ANALYTICS.ENTRY_POINTS.LYRIC_SIDE_REPORT_ISSUE,
      lyric_id: lyricId,
      auth_required: !isAuthenticated,
      transport_type: 'beacon',
    });
  });
})();
