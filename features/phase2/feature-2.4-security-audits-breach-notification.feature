Feature: Security Audits & Breach Notification (Automated)
  As a compliance and security system
  I want to detect breaches and automate notification workflows
  So that contractual and regulatory obligations are met within deadlines

  Background:
    Given automated breach detection is enabled
    And breach notification workflows are configured
    And 72-hour buyer notification deadline is contractual (Section 13.2)
    And 24-hour cyber insurance notification is required
    And executive communication templates are prepared
    And multi-jurisdiction regulatory notifications are configured
    And SIEM integration is active for breach detection
    And deadline tracking with escalation alerts is enabled

  # ============================================================================
  # SCENARIO 1: CRITICAL - 72-Hour Contractual Notification (Section 13.2)
  # ============================================================================
  Scenario: Enforce 72-hour breach notification per contract Section 13.2
    Given a data breach is detected at 2025-10-31 10:00:00 UTC
    When the breach notification protocol is initiated
    Then the notification deadline should be: 2025-11-03 10:00:00 UTC (72 hours)
    And buyer notifications should be scheduled within 72 hours
    And the deadline should be stored in DataBreach table
    And a critical alert should be sent to security@switchboard.com
    And an alert should be sent to legal@switchboard.com
    And an alert should be sent to ciso@switchboard.com
    And the alert should include time remaining: "72.0 hours remaining"
    And this is a CONTRACTUAL requirement (Section 13.2), not FCRA law

  # ============================================================================
  # SCENARIO 2: When Does the Clock Start? (Legal Definition of "Discovery")
  # ============================================================================
  Scenario: Timestamp breach detection as legal "discovery" moment
    Given a security incident is detected at 2025-10-31 10:00:00 UTC
    And the incident is confirmed as data breach at 2025-10-31 12:00:00 UTC
    When the breach notification deadline is calculated
    Then the clock should start at 10:00:00 UTC (initial detection)
    And the detected_at timestamp should be immutable (legal evidence)
    And the 72-hour deadline should be: 2025-11-03 10:00:00 UTC
    And this ensures clear internal definition of "discovery"

  # ============================================================================
  # SCENARIO 3: CRITICAL - 24-Hour Cyber Insurance Notification (or Claim DENIED)
  # ============================================================================
  Scenario: Notify cyber insurance carrier within 24 hours or claim is DENIED
    Given a data breach is detected at 2025-10-31 10:00:00 UTC
    When the breach notification protocol is initiated
    Then cyber insurance carrier should be notified within 24 hours
    And the insurance notification deadline should be: 2025-11-01 10:00:00 UTC
    And the notification should include:
      | field                       | requirement                        |
      | policy_number               | CYB-2025-12345                     |
      | date_time_of_detection      | 2025-10-31 10:00:00 UTC            |
      | estimated_affected_records  | <count>                            |
      | breach_type                 | unauthorized_access, data_breach   |
      | data_types_compromised      | PII, financial data                |
      | estimated_loss_amount       | $<calculated_cost>                 |
      | pre_breach_security_controls| API auth, rate limiting, audit logs|
    And missing 24-hour deadline = insurance claim DENIED
    And this notification should be FIRST step (before customer notification)

  # ============================================================================
  # SCENARIO 4: Executive Summary - Business Impact, NOT Technical Details
  # ============================================================================
  Scenario: Generate executive summary focusing on business metrics for board
    Given a data breach affects 50,000 consumer records
    When the executive summary is generated
    Then the summary should focus on business impact metrics:
      | metric                      | calculation                        |
      | customers_affected          | 50,000                             |
      | estimated_revenue_loss      | <calculated_from_churn>            |
      | estimated_notification_cost | 50,000 × $3 = $150,000             |
      | estimated_legal_cost        | $100,000 (average legal defense)   |
      | potential_regulatory_fines  | <jurisdiction_based>               |
      | reputation_risk_level       | High                               |
      | projected_customer_churn    | 15% (delayed response average)     |
      | total_estimated_cost        | $7.5M (50,000 × $148 + $50K fixed) |
    And the summary should avoid technical jargon (no firewall logs, CVE numbers)
    And the summary should be in business language for board consumption

  # ============================================================================
  # SCENARIO 5: First 24 Hours = Customer Trust Window (15% Trust Drop)
  # ============================================================================
  Scenario: Immediate acknowledgment within 24 hours maintains customer trust
    Given a data breach is detected
    When 24 hours have passed since detection
    Then an immediate holding statement should be sent to affected buyers
    And the statement should acknowledge: "We're investigating a security incident"
    And the statement should NOT wait for complete breach analysis
    And this prevents 15% drop in customer trust metrics from delayed response
    And transparent immediate communication beats complete information later

  # ============================================================================
  # SCENARIO 6: Automated Deadline Tracking with Escalation Alerts
  # ============================================================================
  Scenario: Escalate alerts as breach notification deadline approaches
    Given a data breach was detected 48 hours ago
    And the 72-hour notification deadline is in 24 hours
    When the automated deadline tracker runs
    Then escalation alerts should be triggered:
      | hours_remaining | alert_level | recipients                          |
      | 48              | info        | security@switchboard.com            |
      | 24              | warning     | security + ciso@switchboard.com     |
      | 12              | critical    | security + ciso + ceo@switchboard.com|
      | 2               | emergency   | security + ciso + ceo + legal       |
    And each alert should include: "X hours remaining until contractual deadline"
    And this prevents missing the deadline due to notification fatigue

  # ============================================================================
  # SCENARIO 7: Multi-Jurisdiction Regulatory Notifications
  # ============================================================================
  Scenario: Schedule regulatory notifications for multiple jurisdictions
    Given a data breach affects consumers in US, EU, and California
    When regulatory notifications are scheduled
    Then notifications should be created for:
      | regulatory_body              | jurisdiction     | deadline           |
      | FTC                          | United States    | 72 hours (contract)|
      | EU Data Protection Authority | European Union   | 72 hours (GDPR)    |
      | California Attorney General  | California       | 72 hours (CCPA)    |
    And notification content should be customized per jurisdiction
    And each jurisdiction has unique required information elements

  # ============================================================================
  # SCENARIO 8: Affected Buyer Identification from Audit Logs
  # ============================================================================
  Scenario: Identify affected buyers using audit logs
    Given a data breach was detected at 2025-10-31 10:00:00 UTC
    And the breach window is 24 hours before detection (suspicious activity period)
    When affected buyers are identified
    Then the system should query audit_logs table for:
      | filter                       | value                              |
      | queried_at_start             | 2025-10-30 10:00:00 UTC            |
      | queried_at_end               | 2025-10-31 10:00:00 UTC            |
    And distinct buyer_ids should be extracted from audit logs
    And each affected buyer should receive individual breach notification
    And this ensures all potentially affected buyers are notified

  # ============================================================================
  # SCENARIO 9: Breach Notification Scheduling (Within 1 Hour of Detection)
  # ============================================================================
  Scenario: Schedule buyer notifications within 1 hour of breach detection
    Given a data breach is detected at 2025-10-31 10:00:00 UTC
    And 5 affected buyers are identified
    When breach notifications are scheduled
    Then each buyer should have BreachNotification record created
    And each notification should be scheduled_for: 2025-10-31 11:00:00 UTC (1 hour)
    And the notification_method should be: email (also phone, certified_mail)
    And the notification_deadline should be: 2025-11-03 10:00:00 UTC (72 hours)
    And the status should be: pending

  # ============================================================================
  # SCENARIO 10: False Positive Triage (Avoid Notification Fatigue)
  # ============================================================================
  Scenario: Validate incidents before escalating to breach protocol
    Given a security alert is triggered (potential breach)
    When the automated triage system evaluates the alert
    Then the alert should be classified:
      | classification         | action                              |
      | false_positive         | Log and dismiss                     |
      | potential_incident     | Human validation required           |
      | confirmed_breach       | Escalate to breach protocol         |
    And false positives should NOT trigger breach notifications
    And human validation should occur before contractual notification deadlines
    And this prevents notification fatigue from over-alerting

  # ============================================================================
  # SCENARIO 11: Third-Party Vendor Breach Notification (60% of All Breaches)
  # ============================================================================
  Scenario: Detect and respond to third-party vendor breaches
    Given a third-party vendor signals a data breach
    And the vendor has access to Switchboard consumer data
    When the vendor breach is detected (average 205 days in healthcare sector)
    Then Switchboard should initiate breach protocol
    And the breach should be flagged as vendor_involved: true
    And vendor_name should be recorded for incident report
    And vendor breaches cost $370K more than internal breaches on average
    And automated vendor risk management integration should detect these breaches

  # ============================================================================
  # SCENARIO 12: Breach Cost Estimation for Insurance Claim
  # ============================================================================
  Scenario: Calculate breach cost for cyber insurance claim
    Given a data breach affects 50,000 consumer records
    When the breach cost is estimated
    Then the calculation should be:
      | component                   | formula                            | amount    |
      | cost_per_record             | 50,000 × $148 (IBM 2024 average)   | $7,400,000|
      | fixed_incident_response     | Baseline incident response costs   | $50,000   |
      | total_estimated_cost        | Sum of above                       | $7,450,000|
    And the estimate should be included in insurance notification
    And this supports insurance claim validation

  # ============================================================================
  # SCENARIO 13: Pre-Breach Security Controls Documentation (Insurance Requirement)
  # ============================================================================
  Scenario: Document pre-breach security controls for insurance claim approval
    Given a data breach occurs
    When cyber insurance notification is prepared
    Then the notification must include pre-breach security controls:
      | control                          | description                        |
      | api_key_authentication           | Section 2.1 - Timing-attack resistant|
      | distributed_rate_limiting        | Section 2.2 - Redis-based          |
      | fcra_audit_logging               | Section 2.3 - Immutable 24-month   |
      | tls_1.3_encryption_in_transit    | All API communications encrypted   |
      | aes_256_encryption_at_rest       | AWS RDS encryption enabled         |
      | soc2_type_ii_certified           | Independent audit completed        |
    And insurance carriers require proof of "appropriate controls BEFORE breach"
    And missing documentation = claim DENIED

  # ============================================================================
  # SCENARIO 14: Board Communication Requirements
  # ============================================================================
  Scenario: Communicate breach to board using business metrics
    Given a data breach requires board notification
    When the executive summary is prepared for board meeting
    Then the summary must include business-focused metrics:
      | metric                       | why_board_cares                    |
      | customers_affected           | Reputation risk, customer trust    |
      | estimated_revenue_loss       | Financial impact on bottom line    |
      | potential_regulatory_fines   | Legal/compliance exposure          |
      | reputation_risk_level        | Brand value impact                 |
      | projected_customer_churn     | Customer retention threat          |
      | total_estimated_cost         | Bottom-line financial impact       |
    And the summary should avoid technical jargon
    And CISO should NOT present firewall logs and CVE numbers to board

  # ============================================================================
  # SCENARIO 15: Business Continuity + Incident Response Integration
  # ============================================================================
  Scenario: Coordinate incident response, business continuity, and disaster recovery
    Given a data breach triggers incident response protocol
    When the breach response is initiated
    Then a cross-functional war room should be activated with:
      | role                         | responsibility                     |
      | CISO                         | Incident response coordination     |
      | Legal                        | Regulatory compliance              |
      | PR                           | Crisis communication               |
      | Executive                    | Business continuity decisions      |
      | BC_Coordinator               | Keep operations running            |
    And incident response (IR) = first line of defense
    And business continuity (BC) = keeps operations running during breach
    And disaster recovery (DR) = restores systems after breach
    And all three must coordinate (not siloed)

  # ============================================================================
  # SCENARIO 16: Notification Content Requirements by Jurisdiction
  # ============================================================================
  Scenario Outline: Customize breach notification content per jurisdiction
    Given a data breach requires notification in "<jurisdiction>"
    When the notification is prepared
    Then the notification must include jurisdiction-specific elements:
      | required_element           | description                        |
      | <required_field_1>         | <description_1>                    |
      | <required_field_2>         | <description_2>                    |
      | <required_field_3>         | <description_3>                    |
    And missing required elements = non-compliant notification
    And non-compliant notification = re-notification required

    Examples:
      | jurisdiction  | required_field_1         | required_field_2        | required_field_3           |
      | California    | Date of breach           | Data types compromised  | Free credit monitoring     |
      | GDPR          | DPO contact              | Right to lodge complaint| Data categories affected   |
      | HIPAA         | Patient notification     | Steps to protect PHI    | 60-day notification window |

  # ============================================================================
  # SCENARIO 17: Deadline Miss = Legal Penalties + Contract Breach
  # ============================================================================
  Scenario: Missing 72-hour deadline triggers contract violation
    Given a data breach was detected 73 hours ago
    And the 72-hour contractual deadline has passed (Section 13.2)
    When the notification status is evaluated
    Then a critical alert should be triggered: "CONTRACTUAL DEADLINE MISSED"
    And legal penalties may apply:
      | jurisdiction  | penalty_type                       | amount_range      |
      | California    | Late notification fines            | Variable          |
      | HIPAA         | Per-violation fines                | $100 - $50,000    |
      | Contract      | Contract termination or penalties  | As specified      |
    And the buyer may have grounds to terminate contract (Section 13.2 breach)

  # ============================================================================
  # SCENARIO 18: Automated Breach Detection from SIEM Integration
  # ============================================================================
  Scenario: Detect breaches automatically via SIEM integration
    Given SIEM (Security Information Event Management) is integrated
    And SIEM detects unauthorized access to audit_logs table
    When SIEM triggers a security alert
    Then the automated breach detection system should evaluate the alert
    And the alert should be classified as potential_breach
    And the breach notification protocol should be initiated (if confirmed)
    And this reduces average 205-day detection window for vendor breaches

  # ============================================================================
  # SCENARIO 19: Immutable Breach Incident Records
  # ============================================================================
  Scenario: Ensure breach incident records are immutable for legal evidence
    Given a data breach incident is recorded
    When the SecurityIncident record is created
    Then the record should be immutable (no updates allowed)
    And detected_at timestamp should never be modified (legal evidence)
    And all incident details should be preserved for legal proceedings
    And tamper-evident logging should protect record integrity

  # ============================================================================
  # SCENARIO 20: Revenue Loss Calculation from Customer Churn
  # ============================================================================
  Scenario: Calculate revenue loss from projected customer churn
    Given a data breach affects 1,000 buyers
    And the average buyer revenue is $10,000/year
    And delayed response causes 15% customer churn (industry average)
    When the revenue loss is calculated
    Then the formula should be: 1,000 buyers × $10,000 × 15% churn = $1,500,000
    And this revenue loss should be included in executive summary
    And board needs to understand financial impact on bottom line

  # ============================================================================
  # SCENARIO 21: Notification Cost Calculation
  # ============================================================================
  Scenario: Calculate breach notification costs
    Given a data breach affects 50,000 consumers
    And notification methods include: email, certified mail, credit monitoring
    When notification costs are calculated
    Then the breakdown should be:
      | component                   | cost_per_person | total_cost   |
      | email_notification          | $0.50           | $25,000      |
      | certified_mail              | $3.00           | $150,000     |
      | credit_monitoring_1_year    | $15.00          | $750,000     |
      | total_notification_cost     | $18.50          | $925,000     |
    And this cost should be covered by cyber insurance (if timely notified)

  # ============================================================================
  # SCENARIO 22: Regulatory Fine Estimation
  # ============================================================================
  Scenario Outline: Estimate potential regulatory fines per jurisdiction
    Given a data breach affects "<affected_count>" consumers in "<jurisdiction>"
    When regulatory fines are estimated
    Then the potential fine range should be:
      | jurisdiction  | fine_calculation               | estimated_range  |
      | <jurisdiction>| <fine_formula>                 | $<min> - $<max>  |

    Examples:
      | jurisdiction  | affected_count | fine_formula                     | min       | max         |
      | HIPAA         | 10,000         | $100 - $50,000 per violation     | 100,000   | 50,000,000  |
      | GDPR          | 50,000         | Up to 4% global revenue          | 0         | 20,000,000  |
      | California    | 25,000         | Variable based on delay          | 0         | 5,000,000   |

  # ============================================================================
  # SCENARIO 23: Crisis Communication Plan Activation
  # ============================================================================
  Scenario: Activate pre-approved crisis communication templates
    Given a data breach is detected
    When the first 24-hour response window begins
    Then pre-approved holding statement templates should be available
    And the holding statement should NOT wait for complete investigation
    And the statement should include:
      | element                      | purpose                            |
      | acknowledgment               | "We're investigating an incident"  |
      | timeline                     | "Updates within 48 hours"          |
      | contact_information          | security@switchboard.com           |
      | commitment                   | "Customer data protection priority"|
    And transparent immediate communication maintains customer trust

  # ============================================================================
  # SCENARIO 24: Breach Severity Classification
  # ============================================================================
  Scenario Outline: Classify breach severity based on impact factors
    Given a data breach has characteristics:
      | data_types_compromised     | <data_types>                       |
      | affected_record_count      | <count>                            |
      | external_notification_needed| <external_notification>            |
    When the severity is determined
    Then the severity should be classified as: "<severity>"
    And severity determines escalation level and notification urgency

    Examples:
      | data_types                | count     | external_notification | severity  |
      | hashed_phone_numbers      | 1,000     | false                 | Low       |
      | PII (name, address, DOB)  | 10,000    | true                  | Medium    |
      | SSN, financial data       | 50,000    | true                  | High      |
      | Medical records, PHI      | 100,000   | true                  | Critical  |

  # ============================================================================
  # SCENARIO 25: Dead-Letter Queue for Failed Notifications
  # ============================================================================
  Scenario: Handle failed breach notifications without losing data
    Given a breach notification email fails to send (SMTP error, invalid address)
    When the notification service detects the failure
    Then the failed notification should be written to dead-letter queue
    And an alert should be triggered: "Breach notification failed for buyer <id>"
    And the notification should be retried with exponential backoff
    And manual escalation should occur if retries exhaust
    And ALL breach notifications MUST be delivered (contractual requirement)
