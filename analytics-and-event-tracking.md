Below is a lean GA4 event spec for this homepage work.

I have kept it intentionally tight so it is:

* easy for developers to implement
* clean in GA4
* focused on activation, not vanity tracking

## 1. Diagnosis

The main analytics risk here is not under-tracking. It is messy tracking.

If developers invent event names ad hoc, you will end up with:

* duplicate meanings
* inconsistent parameter names
* hard-to-read reports
* weak attribution across auth redirects

So the goal is one small, stable schema.

---

## 2. Why this matters

You want to answer a small number of questions reliably:

* are people clicking contribution CTAs?
* which homepage sections drive action?
* are anonymous users being blocked by auth?
* are homepage visitors actually starting contribution flows?
* are they submitting?

That means your schema should reflect the user journey, not every UI micro-interaction.

---

## 3. Recommended actions

# GA4 event schema for Bejebeje homepage activation

## Naming conventions

Use:

* `snake_case` for event names
* `snake_case` for parameter names
* short controlled enum values
* no free-text labels unless necessary

Avoid putting visible UI copy directly into analytics. Use stable internal values instead.

---

# Event priority summary

All events in this spec are categorised by implementation priority. Use this table for sprint planning.

| Priority | Event | Section |
|---|---|---|
| **v1 — required** | `homepage_search_submitted` | B.1 |
| **v1 — required** | `homepage_cta_clicked` | B.2 |
| **v1 — required** | `homepage_opportunity_clicked` | B.3 |
| **v1 — required** | `auth_redirect_initiated` | B.4 |
| **v1 — required** | `contribution_flow_started` | B.5 |
| **v1 — required** | `contribution_submitted` | B.6 |
| **v1 — required** | `artist_page_cta_clicked` | C.8 |
| **v1 — required** | `lyric_page_cta_clicked` | C.9 |
| **v1 — best effort** | `sign_up_completed` | B.7 |

`sign_up_completed` is best effort because it depends on whether source context can be reliably recovered after the Cognito redirect round-trip. If it cannot, deprioritise it — `auth_redirect_initiated` and `contribution_flow_started` together cover the critical funnel.

---

# A. Shared parameters

These parameters should be added wherever relevant.

## Common parameters

| parameter name     |    type | example                                                        | notes                                                   |
| ------------------ | ------: | -------------------------------------------------------------- | ------------------------------------------------------- |
| `source_page`      |  string | `home`                                                         | originating page                                        |
| `source_section`   |  string | `hero`, `help_grow_archive`, `where_help_is_needed`, `sidebar` | section where the interaction occurred                  |
| `is_authenticated` | boolean | `true`                                                         | whether user was logged in at time of event             |
| `ui_variant`       |  string | `homepage_v1`                                                  | useful if homepage evolves later                        |
| `destination_type` |  string | `artist_page`, `add_artist_page`, `login_page`                 | where the click is sending the user                     |
| `destination_path` |  string | `/artists/new/selector`                                        | optional; only include if you want path-level debugging |

## Contribution-specific shared parameters

| parameter name      |    type | example                                     | notes                                     |
| ------------------- | ------: | ------------------------------------------- | ----------------------------------------- |
| `contribution_type` |  string | `artist`, `lyric`, `report`                 | controlled enum only                      |
| `entry_point`       |  string | `hero_add_lyric`, `opportunity_artist_card` | stable internal entry label               |
| `auth_required`     | boolean | `true`                                      | whether auth was required for this action |

## Entity-specific shared parameters

| parameter name     |       type | example                   | notes                        |
| ------------------ | ---------: | ------------------------- | ---------------------------- |
| `artist_id`        | string/int | `1234`                    | use internal id if available |
| `artist_slug`      |     string | `sivan-perwer`            | optional but helpful         |
| `lyric_id`         | string/int | `5678`                    | for lyric-specific flows     |
| `lyric_slug`       |     string | `agiri`                   | optional                     |
| `opportunity_type` |     string | `artists_with_few_lyrics` | controlled enum              |

---

# B. Events in scope

## 1. `homepage_search_submitted`

### When to fire

Fire when a user submits a search from the homepage search box.

### Parameters

| parameter name     |    type | required | allowed values / example |
| ------------------ | ------: | -------: | ------------------------ |
| `source_page`      |  string |      yes | `home`                   |
| `source_section`   |  string |      yes | `hero_search`            |
| `is_authenticated` | boolean |      yes | `true` / `false`         |
| `ui_variant`       |  string |      yes | `homepage_v1`            |
| `query_length`     | integer |      yes | `12`                     |
| `result_context`   |  string |       no | `artist_or_lyric`        |

### Notes

Do not send the raw search query text unless you explicitly want that and are comfortable with it.

---

## 2. `homepage_cta_clicked`

### When to fire

Fire when a user clicks any homepage CTA intended to move them toward contribution or signup.

### Parameters

| parameter name     |    type | required | allowed values / example                                                        |
| ------------------ | ------: | -------: | ------------------------------------------------------------------------------- |
| `source_page`      |  string |      yes | `home`                                                                          |
| `source_section`   |  string |      yes | `hero`, `help_grow_archive`, `final_cta`, `sidebar`                             |
| `is_authenticated` | boolean |      yes | `true` / `false`                                                                |
| `ui_variant`       |  string |      yes | `homepage_v1`                                                                   |
| `cta_name`         |  string |      yes | `add_lyric`, `add_artist`, `join_contributor`, `report_issue`, `browse_artists` |
| `entry_point`      |  string |      yes | `hero_add_lyric`, `help_grow_add_artist`                                        |
| `destination_type` |  string |      yes | `add_lyric_page`, `add_artist_page`, `signup_page`, `login_page`                |
| `auth_required`    | boolean |      yes | `true` / `false`                                                                |

### Recommended `cta_name` enum

* `add_lyric`
* `add_artist`
* `join_contributor`
* `report_issue`
* `browse_artists`

---

## 3. `homepage_opportunity_clicked`

### When to fire

Fire when a user clicks an item in the "Where help is needed" module.

### Parameters

| parameter name     |       type | required | allowed values / example                                |
| ------------------ | ---------: | -------: | ------------------------------------------------------- |
| `source_page`      |     string |      yes | `home`                                                  |
| `source_section`   |     string |      yes | `where_help_is_needed`                                  |
| `is_authenticated` |    boolean |      yes | `true` / `false`                                        |
| `ui_variant`       |     string |      yes | `homepage_v1`                                           |
| `opportunity_type` |     string |      yes | `artists_with_few_lyrics`, `new_artists_needing_lyrics` |
| `artist_id`        | string/int |      yes | `1234`                                                  |
| `artist_slug`      |     string |       no | `sivan-perwer`                                          |
| `entry_point`      |     string |      yes | `opportunity_artist_card`                               |
| `destination_type` |     string |      yes | `artist_page`                                           |

### Recommended `opportunity_type` enum for v1

* `artists_with_few_lyrics`
* `new_artists_needing_lyrics`

---

## 4. `auth_redirect_initiated`

### When to fire

Fire when an anonymous user clicks a protected contribution CTA and is redirected to login or sign-up.

### Parameters

| parameter name          |    type | required | allowed values / example                                 |
| ----------------------- | ------: | -------: | -------------------------------------------------------- |
| `source_page`           |  string |      yes | `home`                                                   |
| `source_section`        |  string |      yes | `hero`, `help_grow_archive`, `artist_page`, `lyric_page` |
| `is_authenticated`      | boolean |      yes | `false`                                                  |
| `ui_variant`            |  string |      yes | `homepage_v1`                                            |
| `entry_point`           |  string |      yes | `hero_add_lyric`                                         |
| `contribution_type`     |  string |      yes | `artist`, `lyric`, `report`                              |
| `redirect_target`       |  string |      yes | `login`, `signup`                                        |
| `post_auth_destination` |  string |      yes | `/artists/new/selector`                                  |
| `auth_required`         | boolean |      yes | `true`                                                   |

### Notes

This is one of the most useful events in the whole schema.

It tells you: "users wanted to act, but auth interrupted them."

---

## 5. `contribution_flow_started`

### When to fire

Fire when the user successfully lands on the first page of a contribution flow.

Examples:

* new artist selector opened
* new band form opened
* new individual artist form opened
* new lyric form opened
* lyric report form opened

### Parameters

| parameter name      |       type | required | allowed values / example                                                                     |
| ------------------- | ---------: | -------: | -------------------------------------------------------------------------------------------- |
| `source_page`       |     string |      yes | `home`, `artist_detail`, `lyric_detail`                                                      |
| `source_section`    |     string |       no | `hero`, `help_grow_archive`, `artist_header_cta`                                             |
| `is_authenticated`  |    boolean |      yes | `true`                                                                                       |
| `contribution_type` |     string |      yes | `artist`, `lyric`, `report`                                                                  |
| `entry_point`       |     string |      yes | `hero_add_artist`, `artist_page_add_lyric`                                                   |
| `flow_step`         |     string |      yes | `artist_selector`, `artist_individual_form`, `artist_band_form`, `lyric_form`, `report_form` |
| `artist_id`         | string/int |       no | `1234`                                                                                       |
| `artist_slug`       |     string |       no | `sivan-perwer`                                                                               |
| `lyric_id`          | string/int |       no | `5678`                                                                                       |
| `lyric_slug`        |     string |       no | `agiri`                                                                                      |

### Recommended `flow_step` enum

* `artist_selector`
* `artist_individual_form`
* `artist_band_form`
* `lyric_form`
* `report_form`

---

## 6. `contribution_submitted`

### When to fire

Fire when the user successfully submits a contribution form.

Examples:

* artist submitted
* lyric submitted
* report submitted

### Parameters

| parameter name      |       type | required | allowed values / example                         |
| ------------------- | ---------: | -------: | ------------------------------------------------ |
| `source_page`       |     string |      yes | `home`, `artist_detail`, `lyric_detail`          |
| `source_section`    |     string |       no | `hero`, `help_grow_archive`, `artist_header_cta` |
| `is_authenticated`  |    boolean |      yes | `true`                                           |
| `contribution_type` |     string |      yes | `artist`, `lyric`, `report`                      |
| `entry_point`       |     string |      yes | `hero_add_artist`, `opportunity_artist_card`     |
| `submission_status` |     string |      yes | `submitted`                                      |
| `artist_id`         | string/int |       no | `1234`                                           |
| `artist_slug`       |     string |       no | `sivan-perwer`                                   |
| `lyric_id`          | string/int |       no | `5678`                                           |
| `lyric_slug`        |     string |       no | `agiri`                                          |

### Notes

For this event, `submission_status` will basically always be `submitted`, but having the field can help if you later want `draft_saved` or similar.

---

## 7. `sign_up_completed`

### When to fire

Fire when a user completes sign-up and returns to an app-controlled page where you can emit the event.

### Parameters

| parameter name          |   type | required | allowed values / example                     |
| ----------------------- | -----: | -------: | -------------------------------------------- |
| `source_page`           | string |      yes | `home`                                       |
| `source_section`        | string |       no | `hero`, `sidebar`, `help_grow_archive`       |
| `ui_variant`            | string |      yes | `homepage_v1`                                |
| `entry_point`           | string |      yes | `hero_add_lyric`, `join_contributor_sidebar` |
| `post_auth_destination` | string |      yes | `/artists/new/selector`                      |
| `auth_provider`         | string |      yes | `cognito`                                    |

### Notes

If Cognito makes this awkward, do not force it. It is useful, but less important than `auth_redirect_initiated` and `contribution_flow_started`.

---

# C. Artist and lyric page events (required in v1)

These events correspond to the small CTA additions on the artist detail page and lyric detail page, which are required in this release.

## 8. `artist_page_cta_clicked`

### When to fire

Fire when a user clicks a contribution CTA from the artist detail page.

### Parameters

| parameter name     |       type | required | example                                      |
| ------------------ | ---------: | -------: | -------------------------------------------- |
| `source_page`      |     string |      yes | `artist_detail`                              |
| `source_section`   |     string |      yes | `artist_header`, `empty_state`, `footer_cta` |
| `is_authenticated` |    boolean |      yes | `true` / `false`                             |
| `cta_name`         |     string |      yes | `add_lyric`                                  |
| `entry_point`      |     string |      yes | `artist_header_add_lyric`                    |
| `artist_id`        | string/int |      yes | `1234`                                       |
| `artist_slug`      |     string |       no | `sivan-perwer`                               |
| `auth_required`    |    boolean |      yes | `true`                                       |

---

## 9. `lyric_page_cta_clicked`

### When to fire

Fire when a user clicks a contribution CTA from the lyric detail page.

### Parameters

| parameter name     |       type | required | example                      |
| ------------------ | ---------: | -------: | ---------------------------- |
| `source_page`      |     string |      yes | `lyric_detail`               |
| `source_section`   |     string |      yes | `side_actions`, `footer_cta` |
| `is_authenticated` |    boolean |      yes | `true` / `false`             |
| `cta_name`         |     string |      yes | `report_issue`, `add_lyric`  |
| `entry_point`      |     string |      yes | `lyric_side_report_issue`    |
| `artist_id`        | string/int |       no | `1234`                       |
| `artist_slug`      |     string |       no | `sivan-perwer`               |
| `lyric_id`         | string/int |      yes | `5678`                       |
| `lyric_slug`       |     string |       no | `agiri`                      |
| `auth_required`    |    boolean |      yes | `true`                       |

---

# D. Entry point values

This is important. Developers should not invent these freely.

## Recommended `entry_point` values for homepage

* `hero_add_lyric`
* `hero_add_artist`
* `hero_join_contributor`
* `help_grow_add_lyric`
* `help_grow_add_artist`
* `help_grow_report_issue`
* `sidebar_join_contributor`
* `sidebar_login`
* `opportunity_artist_card`
* `final_cta_add_lyric`
* `final_cta_join_contributor`

## Recommended `entry_point` values for other pages

* `artist_header_add_lyric`
* `artist_empty_state_add_lyric`
* `lyric_side_report_issue`
* `lyric_footer_add_lyric`

Keep this list in code as constants.

---

# E. Example developer payloads

## Example 1: Anonymous user clicks Add lyric from homepage hero

```json
{
  "event": "homepage_cta_clicked",
  "params": {
    "source_page": "home",
    "source_section": "hero",
    "is_authenticated": false,
    "ui_variant": "homepage_v1",
    "cta_name": "add_lyric",
    "entry_point": "hero_add_lyric",
    "destination_type": "signup_page",
    "auth_required": true
  }
}
```

Then immediately after redirect decision:

```json
{
  "event": "auth_redirect_initiated",
  "params": {
    "source_page": "home",
    "source_section": "hero",
    "is_authenticated": false,
    "ui_variant": "homepage_v1",
    "entry_point": "hero_add_lyric",
    "contribution_type": "lyric",
    "redirect_target": "signup",
    "post_auth_destination": "/artists/{artistSlug}/lyrics/new",
    "auth_required": true
  }
}
```

## Example 2: Logged-in user clicks artist from "Where help is needed"

```json
{
  "event": "homepage_opportunity_clicked",
  "params": {
    "source_page": "home",
    "source_section": "where_help_is_needed",
    "is_authenticated": true,
    "ui_variant": "homepage_v1",
    "opportunity_type": "artists_with_few_lyrics",
    "artist_id": 1234,
    "artist_slug": "sivan-perwer",
    "entry_point": "opportunity_artist_card",
    "destination_type": "artist_page"
  }
}
```

## Example 3: Logged-in user starts lyric submission flow from artist page

```json
{
  "event": "contribution_flow_started",
  "params": {
    "source_page": "artist_detail",
    "source_section": "artist_header",
    "is_authenticated": true,
    "contribution_type": "lyric",
    "entry_point": "artist_header_add_lyric",
    "flow_step": "lyric_form",
    "artist_id": 1234,
    "artist_slug": "sivan-perwer"
  }
}
```

## Example 4: User submits lyric

```json
{
  "event": "contribution_submitted",
  "params": {
    "source_page": "artist_detail",
    "source_section": "artist_header",
    "is_authenticated": true,
    "contribution_type": "lyric",
    "entry_point": "artist_header_add_lyric",
    "submission_status": "submitted",
    "artist_id": 1234,
    "artist_slug": "sivan-perwer"
  }
}
```

---

# F. Implementation rules for developers

## Rule 1

Fire events only on meaningful user actions, not on every render.

## Rule 2

Use constants for:

* event names
* `entry_point`
* `source_section`
* `opportunity_type`
* `cta_name`

Do not hardcode strings all over the codebase.

## Rule 3

Preserve source context through auth redirects.

At minimum, preserve:

* `entry_point`
* `contribution_type`
* `post_auth_destination`

A simple querystring or temp session value is fine.

## Rule 4

Do not send lyric text, artist names typed by users, or any other large/freeform content into GA4 unless there is a very deliberate reason.

## Rule 5

If the same interaction both clicks a CTA and starts a flow, fire both only if they happen at distinct moments.
Example:

* click CTA on homepage → `homepage_cta_clicked`
* land on actual form page → `contribution_flow_started`

That is valid and useful.

---

## 4. Risks / tradeoffs

The main risk is going too broad. This schema is deliberately small.

You can always add more later, but if v1 is clean, your reporting will stay usable.

The other risk is poor auth attribution. If source context is not preserved through Cognito, your funnel becomes much weaker. That part is worth getting right.

---

## 5. How to measure success

With the above events, your core funnel becomes:

* homepage pageview
* `homepage_cta_clicked`
* `auth_redirect_initiated`
* `sign_up_completed` if available
* `contribution_flow_started`
* `contribution_submitted`

And your module-value funnel becomes:

* homepage pageview
* `homepage_opportunity_clicked`
* artist pageview
* `artist_page_cta_clicked`
* `contribution_flow_started`
* `contribution_submitted`

That is enough to tell whether the redesign is working.
