// ga4 analytics constants for bejebeje homepage activation
// all event names, entry points, and enum values defined here — no inline strings
(function () {
  'use strict';

  // event names
  var EVENTS = {
    HOMEPAGE_SEARCH_SUBMITTED: 'homepage_search_submitted',
    HOMEPAGE_CTA_CLICKED: 'homepage_cta_clicked',
    HOMEPAGE_OPPORTUNITY_CLICKED: 'homepage_opportunity_clicked',
    AUTH_REDIRECT_INITIATED: 'auth_redirect_initiated',
    CONTRIBUTION_FLOW_STARTED: 'contribution_flow_started',
    CONTRIBUTION_SUBMITTED: 'contribution_submitted',
    SIGN_UP_COMPLETED: 'sign_up_completed',
    ARTIST_PAGE_CTA_CLICKED: 'artist_page_cta_clicked',
    LYRIC_PAGE_CTA_CLICKED: 'lyric_page_cta_clicked',
  };

  // entry_point values
  var ENTRY_POINTS = {
    HERO_ADD_LYRIC: 'hero_add_lyric',
    HERO_ADD_ARTIST: 'hero_add_artist',
    HELP_GROW_ADD_LYRIC: 'help_grow_add_lyric',
    HELP_GROW_ADD_ARTIST: 'help_grow_add_artist',
    HELP_GROW_REPORT_ISSUE: 'help_grow_report_issue',
    OPPORTUNITY_ARTIST_CARD: 'opportunity_artist_card',
    ARTIST_HEADER_ADD_LYRIC: 'artist_header_add_lyric',
    LYRIC_SIDE_REPORT_ISSUE: 'lyric_side_report_issue',
  };

  // source_section values
  var SOURCE_SECTIONS = {
    HERO: 'hero',
    HERO_SEARCH: 'hero_search',
    HELP_GROW_ARCHIVE: 'help_grow_archive',
    WHERE_HELP_IS_NEEDED: 'where_help_is_needed',
    ARTIST_HEADER: 'artist_header',
    SIDE_ACTIONS: 'side_actions',
  };

  // opportunity_type values
  var OPPORTUNITY_TYPES = {
    ARTISTS_WITH_FEW_LYRICS: 'artists_with_few_lyrics',
    NEW_ARTISTS_NEEDING_LYRICS: 'new_artists_needing_lyrics',
  };

  // cta_name values
  var CTA_NAMES = {
    ADD_LYRIC: 'add_lyric',
    ADD_ARTIST: 'add_artist',
    REPORT_ISSUE: 'report_issue',
    BROWSE_ARTISTS: 'browse_artists',
    JOIN_CONTRIBUTOR: 'join_contributor',
  };

  // destination_type values
  var DESTINATION_TYPES = {
    ADD_ARTIST_PAGE: 'add_artist_page',
    LOGIN_PAGE: 'login_page',
    SIGNUP_PAGE: 'signup_page',
    ARTIST_INDEX_PAGE: 'artist_index_page',
    ARTIST_PAGE: 'artist_page',
  };

  // expose constants on window for use by other scripts
  window.BJ_ANALYTICS = {
    EVENTS: EVENTS,
    ENTRY_POINTS: ENTRY_POINTS,
    SOURCE_SECTIONS: SOURCE_SECTIONS,
    OPPORTUNITY_TYPES: OPPORTUNITY_TYPES,
    CTA_NAMES: CTA_NAMES,
    DESTINATION_TYPES: DESTINATION_TYPES,
  };
})();
