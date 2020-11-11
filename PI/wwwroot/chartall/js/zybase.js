//获取当前时间
function fnW(str) {
    var num;
    str >= 10 ? num = str : num = "0" + str;
    return num;
}
var timer = setInterval(function () {
    var date = new Date();
    var year = date.getFullYear(); //当前年份
    var month = date.getMonth(); //当前月份
    var data = date.getDate(); //天
    var hours = date.getHours(); //小时
    var minute = date.getMinutes(); //分
    var second = date.getSeconds(); //秒
    var day = date.getDay(); //获取当前星期几 
    var chineseday=['日','一','二','三','四','五','六']
    var ampm = hours < 12 ? 'am' : 'pm';
    $('#time').html(fnW(hours) + ":" + fnW(minute) + ":" + fnW(second));
    $('#date').html('<span>' + year + '/' + (month + 1) + '/' + data + '</span><span>' + ampm + '</span><span>周' + chineseday[day] + '</span>')
}, 1000)

//获取月份中文
function getCmonth(enm){
    let cname=""
     switch(enm){
       case 'Jan':cname='1月';break;
       case 'Feb':cname='2月';break;
       case 'Mar':cname='3月';break;
       case 'Apr':cname='4月';break;
       case 'May':cname='5月';break;
       case 'Jun':cname='6月';break;
       case 'Jul':cname='7月';break;
       case 'Aug':cname='8月';break;
       case 'Sep':cname='9月';break;
       case 'Oct':cname='10月';break;
       case 'Nov':cname='11月';break;
       case 'Dec':cname='12月';break;
     }
     return cname
}
//格式化数组
function formatterAxis(data,key){
    let axisdata=data.map(item=>{
        return item[key]
    })
    return axisdata
}

//获取部门指标
function getDepzhibiao(){
    const depzhibiao=[{'IE运营部':[0,0,1,1,1,1,1,1,1,1,1,1]},
    {'财务部':[1,1,1,1,1,1,1,1,2,1,1,2]},
    {'采购履行部':[0,0,1,1,1,1,1,1,1,1,1,1]},
    {'仓储物流部':[8,8,8,8,8,8,8,9,9,8,9,9]},
    {'动力部':[4,4,4,4,4,4,4,4,4,4,4,4]},
    {'计划物控部':[2,2,2,2,2,2,2,2,3,2,2,3]},
    {'技术部':[10,10,10,10,10,10,10,10,10,10,10,10]},
    {'人力资源部':[1,1,1,1,1,1,1,1,2,1,1,2]},
    {'设备部':[19,19,19,19,19,19,19,19,19,20,20,20]},
    {'生产二组':[9,5,10,10,10,10,10,10,10,10,10,10]},
    {'生产三组':[10,5,10,10,10,10,10,10,10,10,10,10]},
    {'生产一组':[10,5,10,10,10,10,10,10,10,10,10,10]},
    {'质量部':[24,24,24,25,25,25,25,25,25,25,25,25]},
     {'总经理办公室':[3,3,3,3,3,4,3,4,4,4,4,4]}]
    return depzhibiao
}

//部门顺序显示
function getDepOrder(){
   const depOrder=[
   '质量部',
   '设备部',
   '技术部',
   '生产一组',
   '生产二组',
   '生产三组',
   '生产管理组',
   '仓储物流部',
   '动力部',
   '总经理办公室',
   '计划物控部',
   '财务部',
   '人力资源部',
   '采购履行部',
   'IE运营部'
   ]
  return depOrder


}


//表格数据
let sdate= new Date();
let datelimit=sdate.getMonth()
let trenddata={}
let sumdata=0
let chart5data=[]
let statusdata={}
let statusalldata=[]
let depalldata={}



//获取所有数据
function GetViewData(){
    $.ajax({
        type: "GET",
        url: "/Chart/GetTwoCharts",
        async:false,
        timeout: 60000,
        success: function (data) {
            let dataobj=JSON.parse(data)
            //堆叠柱状图
            chart5data=dataobj.ddata
             //获取部门所有数量
             //获取月度趋势数据
             let montharr=['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec']
            dataobj.ddata.forEach(item=>{
              let  dadval=0
              for(let itemkey in item){
                  if(itemkey!='Department'){
                    if(trenddata.hasOwnProperty(itemkey)){
                        trenddata[itemkey]+=item[itemkey]
                    }else{
                        trenddata[itemkey]=item[itemkey]
                    }
                    dadval+=item[itemkey]
                  }
              }
              depalldata[item.Department]=item[montharr[datelimit]]
              //depalldata.push({name:item.Department,val:item[montharr[datelimit]]})
            })
             //分类数据
            statusalldata=dataobj.sdata 
            statusalldata.forEach(item=>{
                for(let keyitem in item){
                    if(keyitem!='Department'){
                        if(statusdata.hasOwnProperty(keyitem)){
                            statusdata[keyitem]+=item[keyitem]
                        }else{
                            statusdata[keyitem]=item[keyitem]
                        }
                    }
                }
            })
              //2020总提案数据
            for(var item in trenddata){
                sumdata+=trenddata[item]
            }
            $("#sumta").text(sumdata)
            
        },
        complete : function(XMLHttpRequest,status){ 
            　　　　if(status=='timeout'){ 
            　　　　　 ajaxTimeoutTest.abort();
           　　　　　 alert("数据请求超时！请联系管理员！");
           　　　　}
           　　}
    })
 
}

GetViewData()
 

//页面地图数据-坐标维度
var geoCoordMap = {
    '衢州':[118.87,28.93],
    '嘉兴': [120.75, 30.74],
    '泰州': [119.92, 32.45],
    '滁州': [118.31, 32.3],
    '咸阳': [108.71, 34.32],
    '西安': [108.94, 34.34],
    '银川': [106.23, 38.49],
    '云南': [100.27, 25.61],
    '大同': [113.30, 40.07]
};


//下拉框点击出现下拉框内容
$('.select').on('blur', function () {
    $(this).find('.select-ul').hide();
})
$('.select-div').on('click', function () {
if ($(this).siblings('.select-ul').is(":hidden")) {
    $(this).siblings('.select-ul').show();
} else {
    $(this).siblings('.select-ul').hide();
}
})
$('.select-ul').on('click', 'li', function () {
$(this).addClass('active').siblings('li').removeClass('active').parent().hide().siblings('.select-div').html($(this).html());
/*$('#barTitle').text('提案状态分类-'+$(this).html())
//将数据进行切换
for(let i=0;i<statusalldata.length;i++){
    if(statusalldata[i].Department==$(this).html()){
        statusdata=statusalldata[i]
        delete statusdata.Department
        break
    }
}
chart3()*/
//var parentDiv = $(this).parent().parent().parent();
})


//点击进入系统
$("#entersys").on('click',function (){
    window.location.href = "/Home/Index"
     //系统跳转
})

$("#lookinfo").on('click',function (){
    let depval=$("#depselect").text().trim()
     let deparr=['IE运营部','财务部','采购履行部','仓储物流部','动力部','计划物控部','技术部','人力资源部','设备部','生产二组','生产三组','生产一组','质量部','总经理办公室']
    let depindex=1
    deparr.forEach((item,index)=>{
        if(item===depval){
            depindex=index
        }
    })
     let url= "/Chart/XqView?depindex="+depindex
    window.open(url,'_blank')
     //系统跳转
})

 
//年度指标达成率
/*function chart1() {
    //data 为模拟数据
    var sdata = [ {
        name: '年度指标',
        value: 1200
    } ];
    sdata.unshift({name:'完结数',value:sumdata})
    var myChart = echarts.init(document.getElementById('pie'));
    window.addEventListener('resize', function () {
        myChart.resize();
    });
    var str = '';
    for (var i = 0; i < sdata.length; i++) {
        str += '<p><span><i class="legend" style="background:' + startColor[i] + '"></i></span>' + sdata[i].name + '<span class="pie-number" style="color:' + startColor[i] + '">' + sdata[i].value + '</span></p>';
    }
    $('.pie-data').append(str);

var dataArr =Math.floor(sumdata*100/1200) ;
var colorSet = {
    color: '#468EFD'
};
var pieoption = {
    tooltip: {
        formatter: "{a} <br/>{b} : {c}%"
    },

    series: [{
            name: "内部进度条",
            type: "gauge",
            // center: ['20%', '50%'],
            radius: '60%',
            splitNumber: 10,
            axisLine: {
                lineStyle: {
                    color: [
                        [dataArr / 100, colorSet.color],
                        [1, "#111F42"]
                    ],
                    width: 8
                }
            },
            axisLabel: {
                show: false,
            },
            axisTick: {
                show: false,

            },
            splitLine: {
                show: false,
            },
            itemStyle: {
                show: false,
            },
            detail: {
                formatter: function(value) {
                    if (value !== 0) {
                        var num = Math.round(value ) ;
                        return parseInt(num).toFixed(0)+"%";
                    } else {
                        return 0;
                    }
                },
                offsetCenter: [0, 82],
                textStyle: {
                    padding: [0, 0, 0, 0],
                    fontSize: 18,
                    fontWeight: '700',
                    color: colorSet.color
                }
            },
            title: { //标题
                show: true,
                offsetCenter: [0, 46], // x, y，单位px
                textStyle: {
                    color: "#fff",
                    fontSize: 14, //表盘上的标题文字大小
                    fontWeight: 400,
                    fontFamily: 'PingFangSC'
                }
            },
            data: [{
                name: "达成率",
                value: dataArr,
            }],
            pointer: {
                show: true,
                length: '75%',
                radius: '20%',
                width: 8, //指针粗细
            },
            animationDuration: 4000,
        },
        {
            name: '外部刻度',
            type: 'gauge',
            //  center: ['20%', '50%'],
            radius: '70%',
            min: 0, //最小刻度
            max: 100, //最大刻度
            splitNumber: 10, //刻度数量
            startAngle: 225,
            endAngle: -45,
            axisLine: {
                show: true,
                lineStyle: {
                    width: 1,
                    color: [
                        [1, 'rgba(0,0,0,0)']
                    ]
                }
            }, //仪表盘轴线
            axisLabel: {
                show: true,
                color: '#FFFFFF',
                distance: 25,
                formatter: function(v) {
                    switch (v + '') {
                        case '0':
                            return '0';
                        case '10':
                            return '10';
                        case '20':
                            return '20';
                        case '30':
                            return '30';
                        case '40':
                            return '40';
                        case '50':
                            return '50';
                        case '60':
                            return '60';
                        case '70':
                            return '70';
                        case '80':
                            return '80';
                        case '90':
                            return '90';
                        case '100':
                            return '100';
                    }
                }
            }, //刻度标签。
            axisTick: {
                show: true,
                splitNumber: 7,
                lineStyle: {
                    color: colorSet.color, //用颜色渐变函数不起作用
                    width: 1,
                },
                length: -8
            }, //刻度样式
            splitLine: {
                show: true,
                length: -20,
                lineStyle: {
                    color: colorSet.color, //用颜色渐变函数不起作用
                }
            }, //分隔线样式
            detail: {
                show: false
            },
            pointer: {
                show: false
            }
        },
    ]
};
    myChart.setOption(pieoption);
}*/

//年度指标达成率
function chart1(){
    var sdata = [ {
        name: '年度指标',
        value: 1200
    } ];
    
    //加个当前指标
    let allzhibiao=[103,89,106,107,107,108,107,109,112,109,110,113]
    let dqzb=allzhibiao[datelimit]
     let dysl= Object.values(depalldata).reduce((pre,cur)=>{
        pre+=cur
        return pre
    },0)
     sdata.unshift({name:'当前完结数',value:sumdata})
    sdata.unshift({name:'当月指标',value:dqzb})
    sdata.unshift({name:'当月完结数',value:dysl})
    var myChart = echarts.init(document.getElementById('pie'));
    window.addEventListener('resize', function () {
        myChart.resize();
    });
    var str = '';
    let startColor=['#00BFFF','#4169E1','#EE82EE','#808080']
    for (var i = 0; i < sdata.length; i++) {
        str += '<p><span><i class="legend" style="background:' + startColor[i] + '"></i></span>' + sdata[i].name + '<span class="pie-number" style="color:' + startColor[i] + '">' + sdata[i].value + '</span></p>';
    }
    $('.pie-data').append(str);

    //图表option
    var color= ['#00BFFF','#4169E1'];
var dataStyle = {
    normal: {
        label: {
            show: false
        },
        labelLine: {
            show: false
        },
        shadowBlur: 40,
        borderWidth: 10,
        shadowColor: 'rgba(0, 0, 0, 0)' //边框阴影
    }
};
var placeHolderStyle = {
    normal: {
        color: '#59588D',
        label: {
            show: false
        },
        labelLine: {
            show: false
        },
        
    },
    emphasis: {
        color: '#eee'
    }
};
var placeHolderStyle1 = {
    normal: {
        color: '#59588D',
        label: {
            show: false
        },
        labelLine: {
            show: false
        },
        
    },
    emphasis: {
        color: '#eee'
    }
};
let ndsy=1200-sumdata>0?1200-sumdata:0
let dqsy=dqzb-dysl>0?dqzb-dysl:0
let option = {
    // backgroundColor: '#142058',
    title: {
        show: false,
        text: '匹配度',
        x: 'center',
        y: 'center',
        textStyle: {
            fontWeight: 'normal',
            fontSize: 24,
            color: "#fff",
        }
    },
    tooltip: {
        trigger: 'item',
        show: true,
        formatter: "{b} : <br/>{d}%",
        backgroundColor: 'rgba(0,0,0,0.1)', // 背景
        padding: [8, 10], //内边距
        extraCssText: 'box-shadow: 0 0 3px rgba(255, 255, 255, 0.1);', //添加阴影
    },
    series: [ 
        {
            name: 'Line 2',
            type: 'pie',
            clockWise: false,
            radius: ['65%','80%'],
            center:['50%','50%'],
            itemStyle: dataStyle,
            hoverAnimation: false,
            startAngle: 90,
            data: [{
                    value: sumdata,
                    name: '年度完成率',
                    itemStyle: {
                        normal: {
                            color: color[0]
                        }
                    }
                },
                {
                    value: ndsy,
                    name: '',
                    tooltip: {
                        show: false
                    },
                    itemStyle: placeHolderStyle1
                },
            ]
        },
        {
            name: 'Line 2',
            type: 'pie',
            clockWise: false,
            radius: ['65%','80%'],
            center:['50%','50%'],
            itemStyle: dataStyle,
            hoverAnimation: false,
            startAngle: 90,
            data: [{
                    value: sumdata,
                    name: '年度完成率',
                    itemStyle: {
                        normal: {
                            color: color[0]
                        }
                    }
                },
                {
                    value: ndsy,
                    name: '',
                    tooltip: {
                        show: false
                    },
                    itemStyle: placeHolderStyle
                },
            ]
        },
        {
            name: 'Line 3',
            type: 'pie',
            clockWise: false,
            radius: ['35%','50%'],
            center:['50%','50%'],
            itemStyle: dataStyle,
            hoverAnimation: false,
            startAngle: 90,
            data: [{
                    value: dysl,
                    name: '当月指标达成率',
                    itemStyle: {
                        normal: {
                            color: color[1]
                        }
                    }
                },
                {
                    value: dqsy,
                    name: '当月指标达成率',
                    tooltip: {
                        show: false
                    },
                    itemStyle: placeHolderStyle1
                },
            ]
        },
        {
            name: 'Line 3',
            type: 'pie',
            clockWise: false,
            radius: ['35%','50%'],
            center:['50%','50%'],
            itemStyle: dataStyle,
            hoverAnimation: false,
            startAngle: 90,
            data: [{
                    value: dysl,
                    name: '当月指标达成率',
                    itemStyle: {
                        normal: {
                            color: color[1]
                        }
                    }
                },
                {
                    value: dqsy,
                    name: '',
                    tooltip: {
                        show: false
                    },
                    itemStyle: placeHolderStyle
                },
            ]
        }
    ]
};
myChart.setOption(option);
}



chart1()



//月份提案趋势
function chart2(){
    //根据trend生成数据
    let xarr=[]
    for(var item in trenddata){
        xarr.push(getCmonth(item))
    }
    let xtrend=xarr.slice(0,datelimit+1)//为一个Key的数组
    let ytrend=Object.values(trenddata).slice(0,datelimit+1)
    let allzhibiao=[103,89,106,107,107,108,107,109,112,109,110,113]
    let zhibiaoarr=[]
    for(let i=0;i<=datelimit;i++){
        zhibiaoarr.push(allzhibiao[i])
    }
    let option = {
        grid: {
            top: "5%",
            bottom: "10%"//也可设置left和right设置距离来控制图表的大小
        },
        tooltip: {
            trigger: "axis",
            axisPointer: {
                type: "shadow",
                label: {
                    show: true
                }
            }
        },
        xAxis: {
            data:xtrend,
            axisLine: {
                show: true, //隐藏X轴轴线
                lineStyle: {
                    color: '#01FCE3'
                }
            },
            axisTick: {
                show: true //隐藏X轴刻度
            },
            axisLabel: {
                show: true,
                textStyle: {
                    color: "#ebf8ac" //X轴文字颜色
                }
            },
             
        },
        yAxis: [{
                type: "value",
                splitLine: {
                    show: false
                },
                axisTick: {
                    show: true
                },
                axisLine: {
                    show: true,
                    lineStyle: {
                                color: '#FFFFFF'
                                }
                },
                axisLabel: {
                    show: true,
                    textStyle: {
                        color: "#ebf8ac"
                    }
                },
                 
            },
            {
                type: "value",
                gridIndex: 0,
                min: 50,
                max: 100,
                splitNumber: 8,
                splitLine: {
                    show: false
                },
                axisLine: {
                    show: false
                },
                axisTick: {
                    show: false
                },
                axisLabel: {
                    show: false
                },
                splitArea: {
                    show: true,
                    areaStyle: {
                        color: ["rgba(250,250,250,0.0)", "rgba(250,250,250,0.05)"]
                    }
                }
            }
        ],
        series: [{
                name: "月度提案指标",
                type: "line",
                smooth: true, //平滑曲线显示
                showAllSymbol: true, //显示所有图形。
                symbol: "circle", //标记的图形为实心圆
                symbolSize: 10, //标记的大小
                itemStyle: {
                    //折线拐点标志的样式
                    color: "#058cff"
                },
                lineStyle: {
                    color: "#058cff"
                },
                areaStyle:{
                    color: "rgba(5,140,255, 0.2)"
                },
                data: zhibiaoarr
            },
            {
                name: "提案完结数",
                type: "bar",
                barWidth: '20%',
                label: {
                    show: true,
                    position: 'top',
                    color: 'white',
                    fontSize: '10',
                    emphasis: {
                        show: true
                    }
                },
                itemStyle: {
                    normal: {
                        color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [{
                                offset: 0,
                                color: "#00FFE3"
                            },
                            {
                                offset: 1,
                                color: "#4693EC"
                            }
                        ])
                    }
                },
                data: ytrend
            }
        ]
    };
     var myChart = echarts.init(document.getElementById('gdMap'));
     window.addEventListener('resize', function () {
      myChart.resize();
      });
      myChart.setOption(option);
  }

chart2();



//获取当前月各个部门指标
function getCurMonthzb(){
    let depzhibiao=getDepzhibiao()
    let depcurzb={}
    depzhibiao.forEach(item=>{
        let objkey=Object.keys(item)[0]
        depcurzb[objkey]=item[objkey][datelimit]
    })
   return depcurzb
}
 

//部门详细数据表格
function depdetaildata(data){
    let depcurzb=getCurMonthzb()
    //获取部门显示顺序
    let deporder=getDepOrder()
    //直接进行拆解好了
     var str = '<li><span></span><p>部门</p><p>当月数量</p><p>当月指标</p></li>';
     for (var i = 0; i < deporder.length; i++) {
         let listr=''
        if(data[deporder[i]]<depcurzb[deporder[i]]){
            listr='<li><span>' + (i + 1) + '</span><p>' + deporder[i] + '</p><p style="color:red">' +data[deporder[i]] + '</p><p>'+ depcurzb[deporder[i]] + '</p></li>';
        }else{
            listr='<li><span>' + (i + 1) + '</span><p>' + deporder[i] + '</p><p style="color:green">' + data[deporder[i]] + '</p><p>'+ depcurzb[deporder[i]] + '</p></li>';
        }
        str += listr
    }
    $('.ranking-box').html(str);
}
depdetaildata(depalldata)


//中国地图 
let chart4Data = [
    {'name': "嘉兴"}, 
    {'name': "衢州"}, 
    {'name': "泰州"}, 
    {'name': "滁州"}, 
    {'name': "大同"}, 
    {'name': "云南"}, 
    {'name': "咸阳"}, 
    {'name': "银川"}
]
function chart4(data, type, chartType) {
    var s_data = [];
    var myChart = echarts.init(document.getElementById('chart4'));
    window.addEventListener('resize', function () {
        myChart.resize();
    });


    function formtGCData(geoData, data, srcNam, dest) {
        var tGeoDt = [];
        if (dest) {
            for (var i = 0, len = data.length; i < len; i++) {
                if (srcNam != data[i].name) {
                    tGeoDt.push({
                        coords: [geoData[srcNam], geoData[data[i].name]],
                    });
                }
            }
        } else {
            for (var i = 0, len = data.length; i < len; i++) {
                if (srcNam != data[i].name) {
                    tGeoDt.push({
                        coords: [geoData[data[i].name], geoData[srcNam]],
                    });
                }
            }
        }
        return tGeoDt;
    }

    function formtVData(geoData, data, srcNam) {
        var tGeoDt = [];
        for (var i = 0, len = data.length; i < len; i++) {
            var tNam = data[i].name
            if (srcNam != tNam) {
                tGeoDt.push({
                    name: tNam,
                    symbolSize: 2,
                    itemStyle: {
                        normal: {
                            color: '#ffeb40',
                        }
                    },
                    value: geoData[tNam]
                });
            }

        }
        tGeoDt.push({
            name: srcNam,
            value: geoData[srcNam],
            symbolSize: 5,
            itemStyle: {
                normal: {
                    color: '#2ef358',
                }
            }

        });
        return tGeoDt;
    }

    var planePath = 'pin';
 
        s_data.push({
            type: 'lines',
            zlevel: 2,
            effect: {
                show: true,
                period: 1.5,
                trailLength: 0.1,
                //                color: '#2ef358',
                color: '#ffeb40',
                symbol: planePath,
                symbolSize: 6,
                trailLength: 0.5
            },
            lineStyle: {
                normal: {
                    color: '#ffeb40',
                    width: 1,
                    opacity: 0.4,
                    curveness: 0.2
                }
            },
            data: formtGCData(geoCoordMap, data, '西安', false)
        }, {

            type: 'effectScatter',
            coordinateSystem: 'geo',
            zlevel: 2,
            rippleEffect: {
                period: 4,
                scale: 2.5,
                brushType: 'stroke'
            },
            symbol: 'none',
            symbolSize: 4,
            itemStyle: {
                normal: {
                    color: '#fff'
                }
            },

            data: formtVData(geoCoordMap, data, '西安')
        })
    

    var option = {
        tooltip: {
            trigger: 'item',
        },
        geo: {
            map: 'china',
            label: {
                show: true,
                position: 'insideLeft',
                color: 'white',
                fontSize: '10',
                emphasis: {
                    show: true
                }
            },
            roam: true,
            silent: true,
            itemStyle: {
                normal: {
                    areaColor: 'transparent',
                    borderColor: '#0e94eb',
                    shadowBlur: 10,
                    shadowColor: '#0e94ea'
                }
            },
            left: 10,
            right: 10
        },
        series: s_data
    };
   myChart.setOption(option);
}

chart4(chart4Data, 1, '');


//提案状态分类
function chart3() {
    var myChart = echarts.init(document.getElementById('chart3'));
    window.addEventListener('resize', function () {
        myChart.resize();
    });
    let schinese={
        "check":'审核中',
        "opeingy":'实施中',
        "opeingn":'实施超时',
        "mark":'基地评分',
        "zmark":'总部评分'
    }
    let xstatus=[]
   for(let item in statusdata){
    if(item!='Department'){
        xstatus.push(schinese[item])
    }
   }
   let ystatus=Object.values(statusdata)
 
   var  option3 = {
        xAxis: [{
            type: 'category',
            color: '#59588D',
            data: xstatus,
            axisPointer: {
                type: 'line'
            },
            axisLine: {
                lineStyle: {
                    color: '#272456'
                }
            },
            axisLabel: {
                interval:0,
                color: '#59588D',
                textStyle: {
                    fontSize: 12
                },
            },
        }],
        tooltip: {
            trigger: "axis",
            axisPointer: {
                type: "shadow",
                label: {
                    show: true
                }
            }
        },
        yAxis: [{
            axisLabel: {
                formatter: '{value}',
                color: '#59588D',
            },
            axisLine: {
                show: false
            },
            splitLine: {
                lineStyle: {
                    color: '#272456'
                }
            }
        }],
        series: [{
            type: 'bar',
            data: ystatus,
            barWidth: '40%',
            label:{
                show: true,
                position: 'top',
                formatter: '{c}',
                textStyle: {
                    color: '#FFFFFF'
             }},
            itemStyle: {
                normal: {
                    color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [{
                        offset: 0,
                        color: '#41E1D4' // 0% 处的颜色
                    }, {
                        offset: 1,
                        color: '#10A7DB' // 100% 处的颜色
                    }], false),
                    barBorderRadius: [30, 30, 0, 0],
                    shadowColor: 'rgba(0,255,225,1)',
                    shadowBlur: 4,
                }
            }
        }]
    };
    
    myChart.setOption(option3);
    myChart.resize();
}
chart3()

//获取截至到当前月的指标数
function getToNowzb(){
    let depzhibiao=getDepzhibiao()
    let deptonowzb={}
    depzhibiao.forEach(item=>{
        let objkey=Object.keys(item)[0]
        let arrs=item[objkey]
        let depsumzb=0
        for(let i=0;i<datelimit;i++){
            depsumzb+=arrs[i]
        }
        deptonowzb[objkey]=depsumzb
    })
   return deptonowzb
}



//各月度结案对比
function chart5(){
    var myChart = echarts.init(document.getElementById('chart5'));
    window.addEventListener('resize', function () {
        myChart.resize();
    })
    //获取name系列
    let namearr=getDepOrder()
    
    //获取数据系列-需要通过namearr来进行排序
    //chart5data=[Department:'',jan:'']
     let dataarr=[]
    let maxsum=0
    for(let i=0;i<namearr.length;i++){
      for(let j=0;j<chart5data.length;j++){
        if(namearr[i]==chart5data[j]['Department']){
          let valarr=Object.values(chart5data[j])
          let depsum=valarr.reduce((pre,cur,index)=>{
              if(index>0){
                pre+=cur
              }
             return pre
          },0)
          if(maxsum<depsum){
            maxsum=depsum
          }
          dataarr.push(depsum) 
          break
        }
      }
    } 
     

    let maxarr=[]
    for(let x=0;x<namearr.length;x++){
        maxarr.push(maxsum)
    }

    let depcurzb=getToNowzb()
      let depline=namearr.reduce((pre,cur)=>{
         pre.push(depcurzb[cur])
        return pre
    },[])
      
 
   let option = {
        grid: {
            left: '5%',
            right: '5%',
            bottom: '5%',
            top: '10%',
            containLabel: true
        },
        tooltip: {
            trigger: 'axis',
            axisPointer: {
                type: 'none'
            },
            formatter: function(params) {
                return params[0].name + '<br/>' +
                    "<span style='display:inline-block;margin-right:5px;border-radius:10px;width:9px;height:9px;background-color:rgba(36,207,233,0.9)'></span>" +
                    params[0].seriesName + ' : ' + params[0].value + ' 个<br/>'+
                    params[2].name + '<br/>' +
                    "<span style='display:inline-block;margin-right:5px;border-radius:10px;width:9px;height:9px;background-color:rgba(0,255,0,0.9)'></span>" +
                    params[2].seriesName + ' : ' + params[2].value + ' 个<br/>'
            }
        },
        backgroundColor: 'rgb(20,28,52)',
        xAxis: {
            show: false,
            type: 'value'
        },
        yAxis: [{
            type: 'category',
            inverse: true,
            axisLabel: {
                show: true,
                textStyle: {
                    color: '#fff'
                },
            },
            splitLine: {
                show: false
            },
            axisTick: {
                show: false
            },
            axisLine: {
                show: false
            },
            data: namearr
        }, {
            type: 'category',
            inverse: true,
            axisTick: 'none',
            axisLine: 'none',
            show: true,
            axisLabel: {
                textStyle: {
                    color: '#ffffff',
                    fontSize: '12'
                }
            },
            data: dataarr
        }],
        series: [{
                name: '提案个数',
                type: 'bar',
                zlevel: 1,
                itemStyle: {
                    normal: {
                        barBorderRadius: 30,
                        color: new echarts.graphic.LinearGradient(0, 0, 1, 0, [{
                            offset: 0,
                            color: 'rgb(57,89,255,1)'
                        }, {
                            offset: 1,
                            color: 'rgb(46,200,207,1)'
                        }]),
                    },
                },
                barWidth: 20,
                data: dataarr
            },
            {
                name: '背景',
                type: 'bar',
                barWidth: 20,
                barGap: '-100%',
                data: maxarr,
                itemStyle: {
                    normal: {
                        color: 'rgba(24,31,68,1)',
                        barBorderRadius: 30,
                    }
                },
            },
            {
                name: "当前提案指标",
                type: "line",
                //smooth:true,
                showAllSymbol: true, //显示所有图形。
                symbol: "none", //标记的图形为实心圆
                zlevel:2,
                lineStyle: {
                    color: "#00FF00"
                },
                data: depline
            },
        ]
    };
    myChart.setOption(option);
}
 
/*
function chart5(){
    var myChart = echarts.init(document.getElementById('chart5'));
    window.addEventListener('resize', function () {
        myChart.resize();
    })
    //获取series系列
    let date=new Date()
    let datalength = date.getMonth()+1;
     let colorlist=[
    '#7E47FF','#FD5916','#01A4F7','#2EDDCD','#00FFFF','#00FF00',
    '#FFFF00','#FFA500','#D2691E','#FF6347','#FF0000','#808080'
    ]
    let seriesarr=[]
    for(let i=0;i<datalength;i++){
        let seriesval={}
        let itemstyleobj= {
            color: colorlist[i]
        }
        seriesval= {
            name: i+1+'月',
            type: 'bar',
            stack: '总量',
            barWidth: 15,
            itemStyle:{
                normal: itemstyleobj
            },
            data: getDataarr(i)
        }
        seriesarr.push(seriesval)
    }
  
    //获取name系列
    let namearr=chart5data.map(item=>{
        return item.Department
    })
   
    var option = {
        tooltip : {
            trigger: 'axis',
            axisPointer : {            // 坐标轴指示器，坐标轴触发有效
                type : 'shadow'        // 默认为直线，可选为：'line' | 'shadow'
            }
        },
        grid: {
            left: '3%',
            right: '4%',
            bottom: '3%',
            containLabel: true
        },
        xAxis:  {
            type: 'value',
            show:false
        },
        yAxis: {
            type: 'category',
            axisLabel:{
                color:'#FFFFFF'
            },
            data: namearr
        },
        series:seriesarr
    };
     myChart.setOption(option);
}*/

chart5()
