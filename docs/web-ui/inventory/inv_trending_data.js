var bi_options = { title: 'Basic Inventory (Core)' };
var hw_options = { title: 'Hardware Inventory' };
var os_options = { title: 'OS Inventory' };
var sw_options = { title: 'Software Inventory' };
var ug_options = { title: 'User Group Inventory' };

var candlestick_all_options = {legend:'none', title:'Computers sending Inventory'};
var candlestick_out_options = {legend:'none', title: 'Inventory data older than 4 weeks'};

var inv_gauge_table = [
	['Label', 'Value'],
	['Basic',  97.70],
	['HW',  94.88],
	['OS',  94.88],
	['SW',  95.99],
	['UG',  94.88]
];

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
