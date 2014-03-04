// Chart options
var core_options = { title: 'Altiris Agent (7.1.15451.8460)' };
var inv_options = { title: 'Inventory Agent (7.1.7875)' };
var sua_options = { title: 'Software Update Agent (7.1.7875)' };
var swm_options = { title: 'Software Management Agent (7.1.7858)' };
var swv_options = { title: 'Software Virtualization Agent (7.5.522)' };

var bi_options = { title: 'Basic Inventory (Core)' };
var hw_options = { title: 'Hardware Inventory' };
var os_options = { title: 'OS Inventory' };
var sw_options = { title: 'Software Inventory' };
var ug_options = { title: 'User Group Inventory' };


var candlestick_all_options = {legend:'none', title:'Computers sending Inventory'};
var candlestick_out_options = {legend:'none', title: 'Inventory data older than 4 weeks'};

// Gauge control table
var inv_gauge_table = [
	['Label', 'Value'],
	['Basic',  97.70],
	['HW',  94.88],
	['OS',  94.88],
	['SW',  95.99],
	['UG',  94.88]
];
var agent_gauge_table = [
	['Label', 'Value'],
	['Core', 96.58],
	['Inv.', 95.85],
	['Patch', 94.17],
	['Soft.', 96.22]
];
// INVENTORY Line chart tables
var bi_table = [
	['Date', 'Agent #', 'Updated', 'Outdated'],
	['2014-02-25', 13307, 12902, 205],
	['2014-02-26', 13323, 12915, 208],
	['2014-02-27', 13322, 12906, 216],
	['2014-02-28', 13312, 12828, 284],
	['2014-03-01', 13271, 12829, 242],
	['2014-03-02', 13264, 12821, 243]
];
var hw_table = [
	['Date', 'Agent #', 'Updated', 'Outdated'],
	['2014-02-25', 12773, 11578, 1195],
	['2014-02-26', 12808, 11722, 1086],
	['2014-02-27', 12815, 11774, 1041],
	['2014-02-28', 12830, 11823, 1007],
	['2014-03-01', 12802, 11835, 967],
	['2014-03-02', 12795, 11832, 963]
];
var os_table = [
	['Date', 'Agent #', 'Updated', 'Outdated'],
	['2014-02-25', 12787, 11591, 1196],
	['2014-02-26', 12822, 11735, 1087],
	['2014-02-27', 12829, 11787, 1042],
	['2014-02-28', 12844, 11836, 1008],
	['2014-03-01', 12816, 11848, 968],
	['2014-03-02', 12809, 11845, 964]
]
var sw_table = [
	['Date', 'Agent #', 'Updated', 'Outdated'],
	['2014-02-25', 13196, 12759, 237],
	['2014-02-26', 13219, 12759, 260],
	['2014-02-27', 13219, 12749, 270],
	['2014-02-28', 13217, 12715, 502],
	['2014-03-01', 13177, 12715, 262],
	['2014-03-02', 13170, 12712, 258]
];
var ug_table = [
	['Date', 'Agent #', 'Updated', 'Outdated'],
	['2014-02-25', 12773, 11578, 1195],
	['2014-02-26', 12808, 11722, 1086],
	['2014-02-27', 12815, 11774, 1041],
	['2014-02-28', 12830, 11823, 1007],
	['2014-03-01', 12802, 11835, 967],
	['2014-03-02', 12795, 11832, 963]
];
var core_table = [
	['Date', 'Agent #', 'OK', 'NOK'],
	['2014-02-25', 13271, 12428, 443],
	['2014-02-26', 13292, 12494, 398],
	['2014-02-27', 13296, 12558, 338],
	['2014-02-28', 13289, 12586, 303],
	['2014-03-01', 13248, 12584, 264],
	['2014-03-02', 13242, 12833, 259]
];
var inv_table = [
	['Date', 'Agent #', 'OK', 'NOK'],
	['2014-02-25', 12135, 11168, 567],
	['2014-02-26', 13162, 12247, 515],
	['2014-02-27', 13178, 12308, 470],
	['2014-02-28', 13178, 12342, 436],
	['2014-03-01', 13137, 12337, 400],
	['2014-03-02', 13146, 12352, 394]
];
var sua_table = [
	['Date', 'Agent #', 'OK', 'NOK'],
	['2014-02-25',12966,11665, 801],
	['2014-02-26',12984,11729, 855],
	['2014-02-27',12968,11788, 780],
	['2014-02-28',12964,11818, 746],
	['2014-03-01',12943,11833, 710],
	['2014-03-02',12937,11833, 704]
]
var swm_table = [
	['Date', 'Agent #', 'OK', 'NOK'],
	['2014-02-25', 13130, 12184, 446],
	['2014-02-26', 13149, 12248, 401],
	['2014-02-27', 13264, 12453, 311],
	['2014-02-28', 13260, 12492, 268],
	['2014-03-01', 13219, 12486, 233],
	['2014-03-02', 13213, 12486, 227]
]

// Candle stick tables
var candlestick_all_table = [
	['Basic Inventory', 13264, 13271, 13264, 13323],
	['HW Inventory', 12773, 12802, 12795, 12830],
	['OS Inventory', 12787, 12816, 12809, 12844],
	['SW Inventory', 12788, 12811, 12804, 12839],
	['UG Inventory', 12773, 12802, 12795, 12830]
];
var candlestick_out_table = [
	['Basic Inventory', 205, 242, 243, 284],
	['HW Inventory', 963, 967, 963, 1195],
	['OS Inventory', 964, 968, 964, 1196],
	['SW Inventory', 733, 733, 755, 956],
	['UG Inventory', 963, 967, 963, 1195],
];
var candlestick_agent_table = [
	['Core', 13242, 13248, 13242, 13296],
	['Inv.', 12135, 13137, 13146, 13178],
	['Patch', 12937, 12943, 12937, 12984],
	['Soft.', 13130, 13219, 13213, 13264]
];
var candlestick_agent_outdated_table = [
	['Core', 659, 664, 659, 843],
	['Inv.', 794, 800, 794, 967],
	['Patch', 1104, 1110, 1104, 1301],
	['Soft.', 727, 733, 727, 946]
];