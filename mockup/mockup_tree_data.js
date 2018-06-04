var ontJson = [
		{"title": "/rest/global", "extraClasses": "service_ok", "children": [
			{"title": "Dummy", "extraClasses": "doc_ok"},		
		]},			
		{"title": "/rest/user", "extraClasses": "service_ok", "children": [
			{"title": "Dummy", "extraClasses": "doc_ok"},		
		]},
		{"title": "/rest/umlmodel", "extraClasses": "service_ok", "children": [
			{"title": "Dummy", "extraClasses": "doc_ok"},		
		]},
		{"title": "/rest/system", "extraClasses": "service_ok", "children": [
			{"title": "Dummy", "extraClasses": "doc_ok"},		
		]},
		
		{"title": "Virsgrāmata", "extraClasses": "service_ok", "children": [
			{"title": "Dummy", "extraClasses": "doc_ok"},		
		]},
		{"title": "Finanšu pārskati", "extraClasses": "service_ok", "children": [
			{"title": "Dummy", "extraClasses": "doc_ok"},		
		]},
		{"title": "Budžeti", "extraClasses": "service_ok", "children": [
			{"title": "Dummy", "extraClasses": "doc_ok"},		
		]},		
		{"title": "Kontrolings", "extraClasses": "service_ok", "children": [
			{"title": "Dummy", "extraClasses": "doc_ok"},		
		]},
		{"title": "Krājumi", "extraClasses": "service_changed", "children": [
			{"title": "Dummy", "extraClasses": "doc_ok"},	
							{"title": "TNdmNom", "extraClasses": "service_changed", "children": [
			{"title": "TNdmNom.wadl", "extraClasses": "doc_ok"},
			{"title": "TNdmNom.xsd", "extraClasses": "doc_ok"},
			{"title": "attachments", "extraClasses": "service_changed", "children": [
				{"title": "TdmAttachmentBL.xsd", "extraClasses": "doc_ok"},
				{"title": "TNdmNomAttachmentSL.xsd", "extraClasses": "doc_changed"}				
			]},			
		]}
		]},
		{"title": "Norēķini", "extraClasses": "service_ok", "children": [
			{"title": "Dummy", "extraClasses": "doc_ok"},		
		]},
		{"title": "Uzņēmums", "extraClasses": "service_ok", "children": [
			{"title": "Dummy", "extraClasses": "doc_ok"},		
		]},
		{"title": "Banka", "extraClasses": "service_ok", "children": [
			{"title": "Dummy", "extraClasses": "doc_ok"},		
		]},
		{"title": "Kase", "extraClasses": "service_ok", "children": [
			{"title": "Dummy", "extraClasses": "doc_ok"},		
		]},
		{"title": "Cirsmas", "extraClasses": "service_ok", "children": [
			{"title": "Dummy", "extraClasses": "doc_ok"},		
		]},
		{"title": "Pamatlīdzekļi", "extraClasses": "service_ok", "children": [
			{"title": "Dummy", "extraClasses": "doc_ok"},		
		]},
		{"title": "Personāls/algas", "extraClasses": "service_ok", "children": [
			{"title": "Dummy", "extraClasses": "doc_ok"},		
		]},
		{"title": "Ražošana", "extraClasses": "service_ok", "children": [
			{"title": "Dummy", "extraClasses": "doc_ok"},		
		]},
		{"title": "Īpašumu apsaimniekošana", "extraClasses": "service_ok", "children": [
			{"title": "Dummy", "extraClasses": "doc_ok"},		
		]},
		{"title": "Saskaņošana", "extraClasses": "service_ok", "children": [
			{"title": "Dummy", "extraClasses": "doc_ok"},		
		]},
		{"title": "Sistēma", "extraClasses": "service_ok", "children": [
			{"title": "Dummy", "extraClasses": "doc_ok"},		
		]},
		{"title": "Līzings", "extraClasses": "service_ok", "children": [
			{"title": "Dummy", "extraClasses": "doc_ok"},		
		]},
		{"title": "Projektu uzskaite", "extraClasses": "service_ok", "children": [
			{"title": "Dummy", "extraClasses": "doc_ok"},		
		]},
		{"title": "Transporta procesu vadība", "extraClasses": "service_ok", "children": [
			{"title": "Dummy", "extraClasses": "doc_ok"},		
		]},
		{"title": "Aizņēmumi/Galvojumi", "extraClasses": "service_ok", "children": [
			{"title": "Dummy", "extraClasses": "doc_ok"},		
		]}



		
];
		  
		$(function(){
			// using default options
			$("#tree").fancytree({
				source: ontJson
			});
		});