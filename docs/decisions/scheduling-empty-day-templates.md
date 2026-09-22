# Empty schedule day templates

Status: agreed with the user on 2026-09-15.

An enabled day template with no intervals means a non-working day. It must be retained when saving and must override the regular workday template. A disabled special-day template follows the existing fallback rules.

The supplied example uses a daily cycle: 08:00–16:00 and 16:00–02:00 (crosses midnight), a pre-holiday interval 08:00–15:00, and empty Holiday, Saturday and Sunday templates. These empty templates are valid. The overnight interval belongs to its starting day; an empty template adds no new intervals for its day.

No database migration is required. The frontend normalizes HH:mm inputs to HH:mm:ss for the existing TimeOnly JSON contract. Component regression tests cover the example payload; isolated API tests cover the time format. A live database save was not performed.
