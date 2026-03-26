// ga4 contribution_flow_started event and attribution hidden field injection
(function () {
  'use strict';

  var params = new URLSearchParams(window.location.search);
  var entryPoint = params.get('entry_point');
  var contributionType = params.get('contribution_type');
  var sourceSection = params.get('source_section');

  // only process if attribution params are present
  if (!entryPoint) return;

  // 20.3: inject hidden form fields for attribution params into all post forms on the page
  var attributionParams = ['entry_point', 'contribution_type', 'source_section'];

  var forms = document.querySelectorAll('form[method="post"]');
  forms.forEach(function (form) {
    attributionParams.forEach(function (paramName) {
      var value = params.get(paramName);
      if (value && !form.querySelector('input[name="' + paramName + '"]')) {
        var hidden = document.createElement('input');
        hidden.type = 'hidden';
        hidden.name = paramName;
        hidden.value = value;
        form.appendChild(hidden);
      }
    });
  });

  // 20.2: fire contribution_flow_started event
  if (typeof gtag === 'undefined') return;
  if (typeof window.BJ_ANALYTICS === 'undefined') return;

  // determine flow_step from current url path
  var path = window.location.pathname;
  var flowStep = '';

  if (path.indexOf('/artists/new/selector') !== -1) {
    flowStep = 'artist_selector';
  } else if (path.indexOf('/artists/new/individual') !== -1) {
    flowStep = 'artist_individual_form';
  } else if (path.indexOf('/artists/new/band') !== -1) {
    flowStep = 'artist_band_form';
  } else if (path.indexOf('/lyrics/new') !== -1) {
    flowStep = 'lyric_form';
  } else if (path.indexOf('/report') !== -1) {
    flowStep = 'report_form';
  }

  if (!flowStep) return;

  gtag('event', window.BJ_ANALYTICS.EVENTS.CONTRIBUTION_FLOW_STARTED, {
    source_page: sourceSection ? 'home' : '',
    source_section: sourceSection || '',
    is_authenticated: true,
    contribution_type: contributionType || '',
    entry_point: entryPoint,
    flow_step: flowStep,
  });
})();
