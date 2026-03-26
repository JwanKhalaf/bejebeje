// ga4 artist_page_cta_clicked event
(function () {
  'use strict';

  // guard: bail if gtag is not available
  if (typeof gtag === 'undefined') return;
  if (typeof window.BJ_ANALYTICS === 'undefined') return;

  var cta = document.getElementById('artist-add-lyric-cta');
  if (!cta) return;

  cta.addEventListener('click', function () {
    var artistId = parseInt(cta.getAttribute('data-artist-id'), 10);
    var authRequired = cta.getAttribute('data-auth-required') === 'true';

    // use beacon transport since we're navigating away
    gtag('event', window.BJ_ANALYTICS.EVENTS.ARTIST_PAGE_CTA_CLICKED, {
      source_page: 'artist_detail',
      source_section: window.BJ_ANALYTICS.SOURCE_SECTIONS.ARTIST_HEADER,
      is_authenticated: !authRequired,
      cta_name: window.BJ_ANALYTICS.CTA_NAMES.ADD_LYRIC,
      entry_point: window.BJ_ANALYTICS.ENTRY_POINTS.ARTIST_HEADER_ADD_LYRIC,
      artist_id: artistId,
      auth_required: authRequired,
      transport_type: 'beacon',
    });

    // also fire auth_redirect_initiated for anonymous users
    if (authRequired) {
      gtag('event', window.BJ_ANALYTICS.EVENTS.AUTH_REDIRECT_INITIATED, {
        source_page: 'artist_detail',
        source_section: window.BJ_ANALYTICS.SOURCE_SECTIONS.ARTIST_HEADER,
        is_authenticated: false,
        ui_variant: 'homepage_v1',
        entry_point: window.BJ_ANALYTICS.ENTRY_POINTS.ARTIST_HEADER_ADD_LYRIC,
        contribution_type: 'lyric',
        redirect_target: 'login',
        post_auth_destination: cta.href,
        auth_required: true,
        transport_type: 'beacon',
      });
    }
  });
})();
