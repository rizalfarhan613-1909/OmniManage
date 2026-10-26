namespace OmniManage.Api.Seeding;

/// <summary>
/// Curated Indonesian reference data used by the Bogus-powered module seeders.
/// Keeping the names and descriptions curated makes the development fixture
/// read naturally while Bogus supplies numbers, dates and lorem text.
/// </summary>
internal static class DemoDataCatalog
{
    public static string[] FirstNames { get; } =
    [
        "Budi", "Siti", "Agus", "Dewi", "Rudi", "Rina", "Andi", "Maya",
        "Joko", "Putri", "Hendra", "Lina", "Fajar", "Nur", "Eko", "Wulan",
        "Bayu", "Intan", "Dimas", "Ayu", "Reza", "Fitri", "Arif", "Nia",
        "Yoga", "Sari", "Taufik", "Rizki", "Indah", "Dedi", "Yuni", "Bambang",
        "Ratna", "Adi", "Mega", "Ilham", "Citra", "Wahyu", "Anisa", "Gilang",
    ];

    public static string[] LastNames { get; } =
    [
        "Santoso", "Wijaya", "Hartono", "Lestari", "Prasetyo", "Anggraini",
        "Nugroho", "Mahardika", "Gunawan", "Kartika", "Salim", "Suryana",
        "Halim", "Permata", "Saputra", "Kusuma", "Setiawan", "Rahmawati",
        "Firmansyah", "Hidayat", "Susanto", "Purnama", "Wibowo", "Utami",
        "Maulana", "Handayani", "Kurniawan", "Siregar", "Nasution",
        "Simanjuntak", "Sitompul", "Hutapea", "Tanjung", "Lubis", "Pratama",
        "Ardiansyah", "Oktaviani", "Ramadhan", "Syahputra", "Wulandari",
    ];

    public static DemoDepartment[] Departments { get; } =
    [
        new(
            Guid.Parse("0192f5b4-8a00-7000-8000-000000001001"),
            "Engineering",
            ["Software Engineer", "Senior Backend Engineer", "QA Engineer", "DevOps Engineer"]),
        new(
            Guid.Parse("0192f5b4-8a00-7000-8000-000000001002"),
            "Human Resources",
            ["HR Generalist", "Talent Acquisition Specialist", "HR Business Partner"]),
        new(
            Guid.Parse("0192f5b4-8a00-7000-8000-000000001003"),
            "Finance",
            ["Accountant", "Accounts Payable Officer", "Financial Analyst"]),
        new(
            Guid.Parse("0192f5b4-8a00-7000-8000-000000001004"),
            "Sales",
            ["Account Executive", "Sales Operations Analyst", "Key Account Manager"]),
        new(
            Guid.Parse("0192f5b4-8a00-7000-8000-000000001005"),
            "Operations",
            ["Operations Coordinator", "Logistics Officer", "Warehouse Supervisor"]),
        new(
            Guid.Parse("0192f5b4-8a00-7000-8000-000000001006"),
            "Production",
            ["Production Supervisor", "Machine Operator", "Quality Control Inspector"]),
        new(
            Guid.Parse("0192f5b4-8a00-7000-8000-000000001007"),
            "Information Technology",
            ["IT Support Specialist", "Systems Administrator", "Security Engineer"]),
        new(
            Guid.Parse("0192f5b4-8a00-7000-8000-000000001008"),
            "Marketing",
            ["Marketing Specialist", "Content Strategist", "Brand Manager"]),
        new(
            Guid.Parse("0192f5b4-8a00-7000-8000-000000001009"),
            "Procurement",
            ["Procurement Officer", "Purchasing Analyst", "Vendor Manager"]),
        new(
            Guid.Parse("0192f5b4-8a00-7000-8000-000000001010"),
            "Customer Service",
            ["Customer Service Representative", "Support Specialist", "Service Quality Analyst"]),
    ];

    public static string[] ProjectNames { get; } =
    [
        "ERP Core Finance Rollout",
        "Warehouse Automation Upgrade",
        "HRIS Mobile Attendance Launch",
        "Retail POS Modernization",
        "Sales Pipeline Revamp",
        "Jakarta HQ Network Refresh",
        "ISO 27001 Certification Program",
        "Customer Portal Redesign",
        "Fleet Telematics Integration",
        "e-Invoicing Compliance (e-Faktur)",
        "Manufacturing MES Pilot",
        "Vendor Portal Implementation",
        "Data Warehouse Foundation",
        "Omnichannel CRM Rollout",
        "Payroll Automation Phase II",
        "Asset Tracking RFID Deployment",
        "Procurement Workflow Redesign",
        "B2B Marketplace Integration",
        "Logistics Route Optimization",
        "Quality Management System",
        "Employee Self-Service Portal",
        "Cybersecurity Awareness Program",
        "Cloud Migration Assessment",
        "Sustainability Reporting Dashboard",
    ];

    public static string[] TaskTitles { get; } =
    [
        "Requirement gathering",
        "Process mapping",
        "Solution design",
        "API integration",
        "Database schema review",
        "UI component implementation",
        "User acceptance testing",
        "Data migration",
        "Performance tuning",
        "Security review",
        "Deployment preparation",
        "Stakeholder workshop",
        "Documentation update",
        "Regression testing",
        "Production cutover",
        "Post-launch support",
        "Sprint planning",
        "Vendor coordination",
        "Training material preparation",
        "KPI dashboard setup",
    ];

    public static string[] WorkLogDescriptions { get; } =
    [
        "Reviewed requirements and updated the task breakdown.",
        "Implemented the approved design and pushed the changes for review.",
        "Ran integration tests and documented the results.",
        "Coordinated with the vendor on outstanding issues.",
        "Prepared migration scripts and validated the staging data.",
        "Facilitated a workshop with business stakeholders.",
        "Fixed defects reported during user acceptance testing.",
        "Updated technical documentation for the new workflow.",
        "Reviewed pull requests and resolved merge conflicts.",
        "Configured the staging environment for the next sprint.",
        "Analyzed production logs and tuned query performance.",
        "Prepared the cutover checklist and rollback plan.",
        "Trained end users on the updated process.",
        "Followed up on pending approvals and procurement items.",
        "Consolidated feedback into the backlog for prioritization.",
    ];

    public static string[] MaintenanceDescriptions { get; } =
    [
        "Routine preventive maintenance and inspection.",
        "Replace worn hydraulic seal and top up fluid.",
        "Investigate intermittent power failure.",
        "Quarterly HVAC filter cleaning and coil inspection.",
        "Recalibrate sensor and update firmware.",
        "Repair faulty power supply unit.",
        "Annual safety inspection and certification.",
        "Replace damaged display panel.",
        "Lubricate moving parts and verify alignment.",
        "Upgrade memory and storage components.",
    ];

    public static DemoAssetTemplate[] AssetTemplates { get; } =
    [
        new("Dell Latitude 5440 Laptop", "IT Equipment", "IT", 12_000_000m, 22_000_000m),
        new("MacBook Pro 14-inch", "IT Equipment", "IT", 28_000_000m, 36_000_000m),
        new("LG UltraWide 34-inch Monitor", "IT Equipment", "IT", 7_000_000m, 11_000_000m),
        new("Dell PowerEdge R750 Server", "IT Equipment", "IT", 120_000_000m, 180_000_000m),
        new("Ergonomic Office Chair", "Office Furniture", "FURN", 2_500_000m, 5_500_000m),
        new("Height-Adjustable Standing Desk", "Office Furniture", "FURN", 4_000_000m, 8_000_000m),
        new("Meeting Table 12-seater", "Office Furniture", "FURN", 15_000_000m, 25_000_000m),
        new("Toyota Avanza Fleet Van", "Vehicles", "VEH", 220_000_000m, 280_000_000m),
        new("Honda Vario Scooter", "Vehicles", "VEH", 25_000_000m, 35_000_000m),
        new("Toyota 8FG25 Forklift", "Production Machinery", "PROD", 350_000_000m, 450_000_000m),
        new("CNC Milling Machine", "Production Machinery", "PROD", 800_000_000m, 1_200_000_000m),
        new("Industrial Air Compressor", "Production Machinery", "PROD", 60_000_000m, 95_000_000m),
        new("Daikin Split AC 2PK", "HVAC", "HVAC", 8_000_000m, 13_000_000m),
        new("Chiller Unit 50TR", "HVAC", "HVAC", 300_000_000m, 420_000_000m),
        new("CO2 Fire Extinguisher 5kg", "Safety Equipment", "SAFE", 1_200_000m, 2_200_000m),
        new("First Aid Station", "Safety Equipment", "SAFE", 3_000_000m, 6_000_000m),
        new("Dome CCTV Camera 4MP", "Security Equipment", "SEC", 1_500_000m, 3_500_000m),
        new("Access Control Reader", "Security Equipment", "SEC", 4_000_000m, 8_000_000m),
        new("Generator Set 100kVA", "Facilities Equipment", "FAC", 180_000_000m, 260_000_000m),
        new("Industrial Water Pump", "Facilities Equipment", "FAC", 12_000_000m, 25_000_000m),
    ];
}

internal sealed record DemoDepartment(Guid Id, string Name, string[] JobTitles);

internal sealed record DemoAssetTemplate(
    string Name,
    string Category,
    string CodePrefix,
    decimal MinValue,
    decimal MaxValue);
