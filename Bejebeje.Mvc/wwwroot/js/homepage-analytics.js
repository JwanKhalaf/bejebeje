// ga4 event tracking for the bejebeje homepage
(function () {
  'use strict';

  // guard: bail if gtag is not available
  if (typeof gtag === 'undefined') return;

  var ctx = document.getElementById('homepage-analytics-context');
  if (!ctx) return;

  var isAuthenticated = ctx.getAttribute('data-is-authenticated') === 'true';
  var uiVariant = ctx.getAttribute('data-ui-variant') || 'homepage_v1';

  // helper: fire gtag event with beacon transport for pre-navigation events
  var fireBeacon = function (eventName, params) {
    params.send_to = undefined;
    params.transport_type = 'beacon';
    gtag('event', eventName, params);
  };

  // helper: fire standard gtag event
  var fire = function (eventName, params) {
    gtag('event', eventName, params);
  };

  // 19.1 — homepage_search_submitted
  var searchForm = document.getElementById('homepage-search-form');
  var searchInput = document.getElementById('searchTerm');

  if (searchForm && searchInput) {
    searchForm.addEventListener('submit', function () {
      var queryLength = (searchInput.value || '').length;

      fire(window.BJ_ANALYTICS.EVENTS.HOMEPAGE_SEARCH_SUBMITTED, {
        source_page: 'home',
        source_section: window.BJ_ANALYTICS.SOURCE_SECTIONS.HERO_SEARCH,
        is_authenticated: isAuthenticated,
        ui_variant: uiVariant,
        query_length: queryLength,
      });
    });
  }

  // 19.2 — homepage_cta_clicked on all cta links
  var ctaLinks = document.querySelectorAll('[data-analytics-cta]');

  ctaLinks.forEach(function (link) {
    link.addEventListener('click', function () {
      var dataset = link.dataset;

      fireBeacon(window.BJ_ANALYTICS.EVENTS.HOMEPAGE_CTA_CLICKED, {
        source_page: 'home',
        source_section: dataset.sourceSection,
        is_authenticated: isAuthenticated,
        ui_variant: uiVariant,
        cta_name: dataset.ctaName,
        entry_point: dataset.entryPoint,
        destination_type: dataset.destinationType,
        auth_required: dataset.authRequired === 'true',
      });

      // 20.1 — auth_redirect_initiated for anonymous cta clicks
      if (dataset.authRequired === 'true' && !isAuthenticated) {
        fireBeacon(window.BJ_ANALYTICS.EVENTS.AUTH_REDIRECT_INITIATED, {
          source_page: 'home',
          source_section: dataset.sourceSection,
          is_authenticated: false,
          ui_variant: uiVariant,
          entry_point: dataset.entryPoint,
          contribution_type: dataset.contributionType || '',
          redirect_target: dataset.destinationType === 'signup_page' ? 'signup' : 'login',
          post_auth_destination: dataset.postAuthDestination || '',
          auth_required: true,
        });
      }
    });
  });

  // 19.3 — homepage_opportunity_clicked on opportunity cards
  var opportunityLinks = document.querySelectorAll('[data-analytics-opportunity]');

  opportunityLinks.forEach(function (link) {
    link.addEventListener('click', function () {
      var dataset = link.dataset;

      fireBeacon(window.BJ_ANALYTICS.EVENTS.HOMEPAGE_OPPORTUNITY_CLICKED, {
        source_page: 'home',
        source_section: window.BJ_ANALYTICS.SOURCE_SECTIONS.WHERE_HELP_IS_NEEDED,
        is_authenticated: isAuthenticated,
        ui_variant: uiVariant,
        opportunity_type: dataset.opportunityType,
        artist_id: parseInt(dataset.artistId, 10),
        entry_point: window.BJ_ANALYTICS.ENTRY_POINTS.OPPORTUNITY_ARTIST_CARD,
        destination_type: window.BJ_ANALYTICS.DESTINATION_TYPES.ARTIST_PAGE,
      });
    });
  });
})();
