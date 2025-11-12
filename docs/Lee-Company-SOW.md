# Statement of Work

## Power Platform ALM Implementation for The Lee Company

**Date:** November 11, 2025  
**Prepared For:** The Lee Company  
**Prepared By:** Exalents Solutions Inc.

---

## 1. Executive Summary

This Statement of Work outlines the implementation of modern Application Lifecycle Management (ALM) practices for The Lee Company's Power Platform solutions. The engagement will focus on enabling professional developer scenarios, establishing automated CI/CD workflows, and migrating existing Azure DevOps assets and processes to GitHub while supporting the current 3½-week sprint cadence.

**Key Challenge:** The Lee Company's current deployment process relies on nightly batch pipelines that prevent developers from committing and deploying work on-demand and require all team members to complete their work before any deployments can occur. This creates significant bottlenecks and reduces sprint velocity.

**Solution:** This engagement will implement on-demand, developer-initiated commits of completed work and deployments with branch-based isolation, enabling developers to deploy their completed work independently without waiting for others, improving feedback cycles and team productivity.

---

## 2. Current State Assessment

### Environment Architecture

Based on The Lee Company's current configuration:

**Development Environments:**

- Atlas CE Dev Base
- Atlas CE Dev Construction
- Atlas CE Dev Service  
- Atlas CE Dev Int (primary development environment)

**Test Environments:**

- Atlas CE UAT
- Atlas CE QRT

**Production Environment:**

- Atlas CDS Prod

### Development Team

- Small, focused development team
- 3½-week sprint cycles
- Bi-annual production refresh (Spring and Fall)

### Current Tooling

- Azure DevOps for source control and pipelines
- Atlas CE Dev Int as primary development hub

### Solution Ecosystem

- **Solution segmentation**
  - Construction: Lee Construction Configuration Patch; Lee Construction Processes Patch
  - Service: Lee Services Configuration Patch; Lee Services Processes Patch; Lee Services PowerAutomate Patch
  - Integrated Solutions: Lee Integrated Configuration Patch; Lee Integrated Processes Patch; Lee Integrated PowerAutomate Patch
  - Supporting: Lee Services Canvas Apps; Lee Company Security Roles Patch; Lee Company FSP

- **Component mix**: Blend of low-code (Canvas/model-driven apps, Power Automate flows, Dataverse configuration) and pro-developer assets (custom APIs, plugins, PCF controls)
- **Functional separation**: Solutions segmented by business function (Construction, Service) and cross-cutting concerns (Integrated, Security, FSP), indicating parallel workstreams
- **Scale indicator**: Approximately 10+ managed patches across the portfolio, implying mature—but complex—dependency management

### Current Challenges & Bottlenecks

The Lee Company's existing deployment process presents several significant bottlenecks that impact developer productivity and sprint velocity:

1. **Serial Development Constraints** - Developers cannot deploy individual work items independently. All developers must complete their work before any deployments can occur, creating unnecessary coupling between parallel development efforts.

2. **Shared Development Environment** - Developers work in a shared Atlas CE Dev Int environment without isolation, leading to conflicts and the inability to work on features independently.

3. **Nightly Export Process** - Solution exports and source control commits occur on a nightly batch schedule rather than on-demand when work is completed, preventing continuous integration and timely code reviews.

4. **Limited Inner Loop Workflow** - Developers lack a proper inner loop process for local development, testing, and source control integration, forcing all work through the shared environment.

5. **Reduced Sprint Velocity** - The inability to deploy and commit work as it's completed extends feedback cycles and prevents early testing of completed features within the sprint.

6. **Merge Conflicts & Integration Issues** - Batching multiple developers' changes  makes it harder to isolate issues and track changes back to specific work items when problems arise.

**Impact:** These constraints significantly hinder the team's ability to work efficiently within 3½-week sprints and prevent adoption of modern continuous integration practices.

---

## 3. Project Objectives

1. **Modernize Developer Inner Loop** - Replace nightly exports with on-demand commits, and proper source control integration. Allow for deployment of individual features rather than requiring all features to be completed before deployment.

2. **Enable On-Demand Deployments** - Replace nightly batch pipeline execution with on-demand, developer-initiated deployments that can execute immediately when work is ready

3. **Support Independent Developer Workflows** - Implement branching and deployment strategies that allow individual developers to deploy their completed work without waiting for others to finish

4. **Establish Modern ALM Practices** - Implement source control, branching strategies, and automated deployments aligned with Power Platform best practices

5. **Enable Pro-Developer Scenarios** - Support advanced development workflows including PCF controls, plugins with proper build automation

6. **Migrate to GitHub** - Transition repositories and CI/CD pipelines from Azure DevOps to GitHub Actions with improved automation capabilities

7. **Streamline Environment Management** - Automate solution deployment across Dev → Test → Production environments with proper isolation between developer work streams

8. **Improve Sprint Velocity** - Reduce feedback cycles and enable continuous integration within the existing 3½-week sprint cadence

---

## 4. High-Level Requirements

### 4.1 Developer Inner Loop & Source Control Integration

**Addressing Current Bottlenecks:**
The new developer workflow will replace the  nightly export model with modern inner loop practices:

- **On-Demand Source Control** - Developers commit changes and create pull requests when work is complete, not on a nightly schedule
- **Local Development Support** - Enable local build, test, and validation before committing to source control
- **Feature Branch Workflow** - Each developer works in isolated feature branches with independent integration paths
- **Continuous Integration** - Automated builds and tests run on every commit/pull request, providing immediate feedback
- **Integrated Code-First Builds** - Plugin and PCF source code  are source controlled with solutions and built / tested during build process

### 4.2 Source Control & Repository Structure

- Plan for cut over from Azure DevOps to GitHub
- Establish repository structure for Power Platform solutions
- Implement branching strategy supporting multiple developers
- Configure appropriate access controls and permissions

### 4.3 CI/CD Pipeline Implementation

**Addressing Current Bottlenecks:**
The new pipeline architecture will eliminate nightly batch processing and enable independent developer workflows through:

- **On-Demand Execution** - Pipelines trigger automatically on code commits or can be manually initiated by developers when work is ready
- **Branch-Based Isolation** - Each developer works in feature branches with independent deployment capabilities
- **Parallel Development Support** - Multiple developers can deploy simultaneously to different dev environments without blocking each other

**Build Pipelines:**

- PCF control compilation and bundling
- Plugin/custom workflow assembly builds
- Client-side hooks (TypeScript/JavaScript) compilation
- Solution packaging and validation
- Automated testing and quality gates

**Deployment Pipelines:**

- Work item based shipping of features when work is completed
- Individual developer deployment workflows (no waiting for others to complete)
- Promotion workflows to Test environments (UAT/QRT)
- Controlled production deployment process with approvals
- Environment variable and connection reference management

### 4.4 Solution Management

- Configure solution layering and dependencies
- Implement automated solution versioning
- Establish configuration data management
- Support for managed identities and secure authentication

### 4.5 Developer Experience

- Local development environment setup documentation
- Build and deployment tooling configuration
- Integration with existing sprint workflow
- Support for concurrent development across multiple Dev environments

---

## 5. Scope of Work

### Phase 1: Foundation & Migration

**Deliverables:**

- GitHub organization and repository setup
- Migration of existing codebases from Azure DevOps
- Repository structure and branching strategy documentation
- Initial access controls and team permissions

**Estimated Effort:** 40-50 hours

### Phase 2: Developer Inner Loop & Source Control Integration

**Deliverables:**

- Developer workstation setup and tooling configuration
- Local build and test environment setup
- Source control workflow documentation (branching, commits, pull requests)
- Integration with Power Platform CLI and development tools
- Pre-commit hooks and local validation
- Developer onboarding guide for daily workflows
- Pull request templates and code review process

**Estimated Effort:** 45-55 hours

### Phase 3: Build Automation

**Deliverables:**

- GitHub Actions workflows for PCF controls
- Plugin and custom workflow assembly builds
- Solution packaging automation
- Build validation and testing gates

**Estimated Effort:** 50-60 hours

### Phase 4: Deployment Automation

**Deliverables:**

- Automated deployment to Dev environments
- Test environment promotion workflows
- Production deployment process (with approvals)
- Environment variable and connection reference automation
- Configuration data deployment

**Estimated Effort:** 60-70 hours

### Phase 5: Integration & Training

**Deliverables:**

- Integration with 3½-week sprint cycle
- Developer documentation and runbooks
- Team training / office hours for questions
- Support for first production deployment
- Handoff and knowledge transfer

**Estimated Effort:** 30-40 hours

---

## 6. Project Estimates

### Time & Duration

- **Total Duration:** 10-12 weeks
- **Total Effort:** 225-265 hours (based on similar project experience)
- **Sprint Alignment:** Work will be structured to align with client's 3½-week sprints

### Cost Estimate

- **Professional Services:** [225-265 hours × $190]

### Assumptions

- Lee Company will provide necessary access to all environments
- Development team (Jared, Eric, Chris) will be available for collaboration
- Existing Azure DevOps assets are accessible for reference and for cut over
- GitHub organization provisioned and access configured
- Service principals/Managed Identities can be created for automation

---

## 7. Out of Scope

The following items are excluded from this engagement:

- Remediation of existing solution technical debt outside of ALM specific refactoring needed
- Power Platform governance policy implementation
- Advanced analytics or monitoring solutions
- Infrastructure provisioning (environments assumed to exist)
- Third-party integration development beyond standard ALM practices
- Provisions for bi-annual environment refreshes

---

## 8. Success Criteria

This engagement will be considered successful when:

1. All repositories successfully migrated to GitHub
2. Build pipelines execute successfully for all solution components
3. **On-demand deployments functional** - Developers can initiate deployments immediately when work is complete (eliminating nightly batch delays)
4. **Independent deployment capability** - Individual developers can deploy their work without waiting for others to finish their tasks
5. Automated deployments function across Dev → Test → Production
6. At least one complete solution deployed to production using new process
7. Documentation and training materials delivered

---

## 9. Risks & Mitigation

| Risk | Impact | Mitigation |
|------|--------|------------|
| Environment access delays | Schedule impact | Early credential/access provisioning |
| Complex solution dependencies or components | Build failures | Thorough discovery and testing |
| Holiday season stakeholder availability | Project delays, reduced collaboration | Schedule critical milestones before/after holidays; frontload discovery |
| Team availability | Knowledge transfer gaps | Record training sessions, comprehensive docs |

---

## 10. Next Steps

1. Review and approve this Statement of Work
2. Schedule project kickoff meeting
3. Provision GitHub organization/repositories
4. Grant environment access to consulting team (discuss Azure resource provisioning)
5. Begin Phase 1 activities

---

## 11. Acceptance & Approval

**The Lee Company:**

Name: ___________________________  
Title: ___________________________  
Signature: _______________________  
Date: ___________________________

**Exalents Solutions Inc.**

Name: ___________________________  
Title: ___________________________  
Signature: _______________________  
Date: ___________________________

---

*This Statement of Work is valid for 30 days from the date of issuance.*
