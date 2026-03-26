// ga4 contribution_submitted event — fires on post-submission landing pages
(function () {
  'use strict';

  // guard: bail if gtag is not available
  if (typeof gtag === 'undefined') return;
  if (typeof window.BJ_ANALYTICS === 'undefined') return;

  var ctx = document.getElementById('contribution-submitted-context');
  if (!ctx) return;

  var entryPoint = ctx.getAttribute('data-entry-point');
  if (!entryPoint) return;

  var contributionType = ctx.getAttribute('data-contribution-type') || '';
  var sourceSection = ctx.getAttribute('data-source-section') || '';

  gtag('event', window.BJ_ANALYTICS.EVENTS.CONTRIBUTION_SUBMITTED, {
    source_page: sourceSection ? 'home' : '',
    source_section: sourceSection,
    is_authenticated: true,
    contribution_type: contributionType,
    entry_point: entryPoint,
    submission_status: 'submitted',
  });
})();
