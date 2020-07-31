$(function () {
    //getAlldata()
    let caonima =$("#realdep").text()
    getAlldata(caonima)
    //获取所有数据
    function getAlldata(nowdep) {
        //获取数据
        $.ajax({
            type: "GET",
            url: "/Chart/GetXqCharts",
            success: function (res) {
                 let resobj = JSON.parse(res)
                 if (resobj.code == 0) {
                    let ndate = new Date();
                    let month = ndate.getMonth()
                    let yuefen = ['一月', '二月', '三月', '四月', '五月', '六月', '七月', '八月', '九月', '十月', '十一月', '十二月']
                    //let montnen = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec']
                    let nowmonth = yuefen.slice(0, month+1)
                    let depzhibiao = [{
                            dep:'IE运营部',
                            value: [0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1],
                            sum:10
                        },
                        {
                            dep:'财务部',value:[1, 1, 1, 1, 1, 1, 1, 1, 2, 1, 1, 2],sum:14
                        },
                        {
                            dep:'采购履行部',value:[0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1],sum:10
                        },
                        {
                            dep:'仓储物流部',value:[8, 8, 8, 8, 8, 8, 8, 9, 9, 8, 9, 9],sum:100
                        },
                        {
                            dep:'动力部',value: [4,4,4,4,4,4,4,4,4,4,4,4],sum:48
                        },
                        {
                            dep:'计划物控部',value: [2, 2, 2, 2, 2, 2, 2, 2, 3, 2, 2, 3],sum:26
                        },
                        {
                            dep:'技术部',value: [10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10],sum:120
                        },
                        {
                            dep:'人力资源部',value: [1, 1, 1, 1, 1, 1, 1, 1, 2, 1, 1, 2],sum:14
                        },
                        {
                            dep:'设备部',value: [19, 19, 19, 19, 19, 19, 19, 19, 19, 20, 20, 20],sum:231
                        },
                        {
                            dep:'生产二组',value: [9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9],sum:108
                        },
                        {
                            dep:'生产三组',value: [9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9],sum:108
                        },
                        {
                            dep:'生产一组',value: [9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9],sum:108
                        },
                        {
                            dep:'质量部',value: [24, 24, 24, 25, 25, 25, 25, 25, 25, 25, 25, 25],sum:297
                        },
                        {
                            dep:'总经理办公室',value:[3, 3, 3, 3, 3, 4, 3, 4, 4, 4, 4, 4],sum:42
                        }
                    ]
                     //当前部门数据
                    let depdata =Object.values(resobj.ddata.filter(item => {
                        return item.Department == nowdep
                    })[0]) 
                    
                    //月度趋势
                    let depdataarr=depdata.slice(1,month+2)
                    //部门每月指标
                    let curdepzb = depzhibiao.filter(item => {
                        return item.dep === nowdep
                    })
                    let yuezhibiao=curdepzb[0].value.splice(0,month+1)
                     
                    echarts_1(nowmonth, depdataarr, yuezhibiao)
            
            
            
                    //指标达成率
                    //部门当月指标
                    let dqzhibiao=yuezhibiao[month]
                    //部门当月数量
                    let curmonthval=depdata[month+1]
                    //部门指标总数
                    let depyearzb=curdepzb[0].sum
                    //部门提案总数
                    let sumdepdata =depdata.reduce((pre,cur,index)=>{
                        if(index>1){
                            pre+=cur
                        }
                        return pre
                    },0)
                    echarts_4(curmonthval, dqzhibiao, sumdepdata,depyearzb);
            
            
                    //提案分类数据
                    let fenlei = ['审核中', '实施中', '基地评分', '总部评分', '实施超时']
                    let fenleirel=resobj.sdata.filter(item=>{return item.Department === nowdep})
                    let fenleidata =Object.values(fenleirel[0]) 
                    echarts_2(fenlei, fenleidata);
            
                    //中间数据
                    $("#zbval").html(depyearzb)
                    $("#allval").html(sumdepdata)
            
                   //提案分数分布
                   let markrange=["0-40","41-50","51-60",">60"]
                   let depmark=resobj.mdata.filter(item=>{return item.Department === nowdep})
                   let markval=Object.values(depmark[0])
                    echarts_3(markrange,markval)
            
                   //最后表格数据
                   let tabledata=resobj.goodtopic
                   tabledata.forEach((item,index)=>{
                       let tr=""
                       let trtitle='<a href="/Topic/Index/' + item.Id + '"' + ' target="_blank" style="color:white">'
                       if(index==0){
                        tr="<tr>"+
                        "<td class='item1'>"+trtitle+item.Title+"</a></td>"+
                        "<td class='item2'>"+item.ZongbuMark+"</td>"+
                        "<td class='item3'>"+item.CreateOn.substr(0,10)+"</td>"+
                        "</tr>"
                       }else{
                        tr="<tr>"+
                        "<td>"+trtitle+item.Title+"</a></td>"+
                        "<td>"+item.ZongbuMark+"</td>"+
                        "<td>"+item.CreateOn.substr(0,10)+"</td>"+
                        "</tr>"
                       }
                       $("#tablebody").append(tr)
                    })




                }
            }
        })




    }


    //部门月度趋势
    function echarts_1(data1, data2, data3) {
        var myChart = echarts.init(document.getElementById('echart1'));
        option = {
            tooltip: {
                trigger: 'axis',
                axisPointer: {
                    type: 'shadow'
                }
            },
            grid: {
                left: '0%',
                top: '10%',
                right: '0%',
                bottom: '2%',
                containLabel: true
            },
            xAxis: [{
                type: 'category',
                data: data1,
                axisLine: {
                    show: true,
                    lineStyle: {
                        color: "rgba(255,255,255,.1)",
                        width: 1,
                        type: "solid"
                    },
                },
                axisTick: {
                    show: false,
                },
                axisLabel: {
                    interval: 0,
                    show: true,
                    splitNumber: 15,
                    textStyle: {
                        color: "#FFFFFF",
                        fontSize: '12',
                    },
                },
            }],
            yAxis: [{
                type: 'value',
                axisLabel: {
                    //formatter: '{value} %'
                    show: true,
                    textStyle: {
                        color: "rgba(255,255,255,.6)",
                        fontSize: '12',
                    },
                },
                axisTick: {
                    show: false,
                },
                axisLine: {
                    show: true,
                    lineStyle: {
                        color: "rgba(255,255,255,.1	)",
                        width: 1,
                        type: "solid"
                    },
                },
                splitLine: {
                    lineStyle: {
                        color: "rgba(255,255,255,.1)",
                    }
                }
            }],
            series: [{
                    type: 'bar',
                    data: data2,
                    barWidth: '35%', //柱子宽度
                    label: {
                        show: true,
                        position: 'top',
                        color: '#FFFFFF'
                    },
                    itemStyle: {
                        normal: {
                            color: '#48D1CC',
                            opacity: 1,
                            barBorderRadius: 5,
                        }
                    }
                },
                {
                    type: 'line',
                    data: data3,
                    symbol: 'none',
                    lineStyle: {
                        color: '#48D1CC'
                    }

                }

            ]
        };

        // 使用刚指定的配置项和数据显示图表。
        myChart.setOption(option);
        window.addEventListener("resize", function () {
            myChart.resize();
        });
    }

    //提案分类
    function echarts_2(data1, data2) {
        data2.shift()
        // 基于准备好的dom，初始化echarts实例
        var myChart = echarts.init(document.getElementById('echart2'));

        let option = {
             tooltip: {
                trigger: 'axis',
                axisPointer: {
                    type: 'shadow'
                }
            },
            grid: {
                left: '0%',
                top: '10%',
                right: '0%',
                bottom: '2%',
                containLabel: true
            },
            xAxis: [{
                type: 'category',
                data: data1,
                axisLine: {
                    show: true,
                    lineStyle: {
                        color: "rgba(255,255,255,.1)",
                        width: 1,
                        type: "solid"
                    },
                },
                axisTick: {
                    show: false,
                },
                axisLabel: {
                    interval: 0,
                    // rotate:50,
                    show: true,
                    splitNumber: 15,
                    textStyle: {
                        color: "#FFFFFF",
                        fontSize: '12',
                    },
                },
            }],
            yAxis: [{
                type: 'value',
                axisLabel: {
                    //formatter: '{value} %'
                    show: true,
                    textStyle: {
                        color: "rgba(255,255,255,.6)",
                        fontSize: '12',
                    },
                },
                axisTick: {
                    show: false,
                },
                axisLine: {
                    show: true,
                    lineStyle: {
                        color: "rgba(255,255,255,.1	)",
                        width: 1,
                        type: "solid"
                    },
                },
                splitLine: {
                    lineStyle: {
                        color: "rgba(255,255,255,.1)",
                    }
                }
            }],
            series: [{
                    type: 'bar',
                    data: data2,
                    barWidth: '35%', //柱子宽度
                    label: {
                        show: true,
                        position: 'top',
                        color: 'white'
                    },
                    itemStyle: {
                        normal: {
                            color: '#90EE90', //#90EE90	 #27d08a
                            opacity: 1,
                            barBorderRadius: 5,
                        }
                    }
                }

            ]
        };

        // 使用刚指定的配置项和数据显示图表。
        myChart.setOption(option);
        window.addEventListener("resize", function () {
            myChart.resize();
        });
    }

    //分数分布
    function echarts_3(data1,data2) {
        let newdata1=[]
        let newdata2=[]
        let dqval=0
        let sum=0
        for(let i=1;i<data2.length;i++){
            if(data2[i]>0){
                newdata1.push(data1[i-1])
                newdata2.push(data2[i])
                sum+=data2[i]
            }
        }
        dqval=sum/30
 
        
        var myChart = echarts.init(document.getElementById('echart3'));
        var img = 'data:image/png;base64,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';
         

        var data = [];
        //var color = ['#00ffff', '#00ffff', '#00ffff', '#00ffff','#00ffff']
        var color = ['#00ffff', '#1E90FF', '#FFA500', '#ffe000','#ffe000']
        for (let i = 0; i < newdata2.length; i++) {
            data.push({
                value: newdata2[i],
                name: newdata1[i],
                itemStyle: {
                    normal: {
                        borderWidth: 5,
                        shadowBlur: 20,
                        borderColor: color[i],
                        shadowColor: color[i]
                    }
                }
            }, {
                value:dqval,
                name: '',
                itemStyle: {
                    normal: {
                        label: {
                            show: false
                        },
                        labelLine: {
                            show: false
                        },
                        color: 'rgba(0, 0, 0, 0)',
                        borderColor: 'rgba(0, 0, 0, 0)',
                        borderWidth: 0
                    }
                }
            });
        }
        var seriesOption = [{
            name: 'markpie',
            type: 'pie',
            clockWise: false,
            radius: [65, 70],
            hoverAnimation: false,
            itemStyle: {
                normal: {
                    label: {
                        show: true,
                        position: 'outside',
                        color: '#F5F5F5',
                        formatter: function (params) {
                            var percent = 0;
                            var total = 0;
                            for (var i = 0; i < newdata2.length; i++) {
                                total +=newdata2[i]
                            }
                            percent = ((params.value / total) * 100).toFixed(0);
                            if (params.name !== '') {
                                return '分数范围:' + params.name + '\n'+ '数据:' + params.value + '个,' + percent + '%'
                                 
                            } else {
                                return '';
                            }
                        },
                    },
                    labelLine: {
                        length: 10,
                        length2: 30,
                        show: true,
                        color: '#00ffff'
                    }
                }
            },
            data: data
        }];
        let option = {
            color: color,
            title: {
                text: '分数分布',
                top: '45%',
                textAlign: "center",
                left: "49%",
                textStyle: {
                    color: '#fff',
                    fontSize: 15,
                    fontWeight: '400'
                }
            },
            graphic: {
                elements: [{
                    type: "image",
                    z: 3,
                    style: {
                        image: img,
                        width: 100,
                        height: 100
                    },
                    left: 'center',
                    top: 'center',
                    position: [100, 100]
                }]
            },
            tooltip: {
                show: false
            },
            toolbox: {
                show: false
            },
            series: seriesOption
        }

        myChart.setOption(option);
        window.addEventListener("resize", function () {
            myChart.resize();
        });
    }

    //提案达成率
    function echarts_4(dysl, zbsl, depsl,allsl) {
        let monthrate=Math.floor(dysl*100/zbsl)
        let deprate=Math.floor(depsl*100/allsl)

         var myChart = echarts.init(document.getElementById('echart4'));


        var placeHolderStyle = {
            normal: {
                label: {
                    show: false
                },
                labelLine: {
                    show: false
                },
                color: "rgba(0,0,0,0)",
                borderWidth: 0
            },
            emphasis: {
                color: "rgba(0,0,0,0)",
                borderWidth: 0
            }
        };


        var dataStyle = {
            normal: {
                formatter: '{c}%',
                position: 'center',
                show: true,
                textStyle: {
                    fontSize: '14',
                    fontWeight: 'normal',
                    color: '#FFFFFF'
                }
            }
        };


        let option = {
            title: [{
                text: '当月指标达成率',
                left: '29.8%',
                top: '75%',
                textAlign: 'center',
                textStyle: {
                    fontWeight: 'normal',
                    fontSize: '14',
                    color: '#FFFFFF',
                    textAlign: 'center',
                },
            }, {
                text: '年度达成率',
                left: '70%',
                top: '75%',
                textAlign: 'center',
                textStyle: {
                    color: '#FFFFFF',
                    fontWeight: 'normal',
                    fontSize: '14',
                    textAlign: 'center',
                },
            }],

            //第一个图表
            series: [{
                    type: 'pie',
                    hoverAnimation: false, //鼠标经过的特效
                    radius: ['60%', '75%'],
                    center: ['30%', '40%'],
                    startAngle: 225,
                    labelLine: {
                        normal: {
                            show: false
                        }
                    },
                    label: {
                        normal: {
                            position: 'center'
                        }
                    },
                    data: [{
                            value: 100,
                            itemStyle: {
                                normal: {
                                    color: '#E1E8EE'
                                }
                            },
                        }, {
                            value: 33,
                            itemStyle: placeHolderStyle,
                        },

                    ]
                },
                //上层环形配置
                {
                    type: 'pie',
                    hoverAnimation: false, //鼠标经过的特效
                    radius: ['60%', '75%'],
                    center: ['30%', '40%'],
                    startAngle: 225,
                    labelLine: {
                        normal: {
                            show: false
                        }
                    },
                    label: {
                        normal: {
                            position: 'center'
                        }
                    },
                    data: [{
                            value: monthrate,
                            itemStyle: {
                                normal: {
                                    color: '#48D1CC'
                                }
                            },
                            label: dataStyle,
                        }, {
                            value: 33,
                            itemStyle: placeHolderStyle,
                        },

                    ]
                },


                //第二个图表
                {
                    type: 'pie',
                    hoverAnimation: false,
                    radius: ['60%', '75%'],
                    center: ['70%', '40%'],
                    startAngle: 225,
                    labelLine: {
                        normal: {
                            show: false
                        }
                    },
                    label: {
                        normal: {
                            position: 'center'
                        }
                    },
                    data: [{
                            value: 100,
                            itemStyle: {
                                normal: {
                                    color: '#E1E8EE'
                                }
                            },

                        }, {
                            value: 33,
                            itemStyle: placeHolderStyle,
                        },

                    ]
                },

                //上层环形配置
                {
                    type: 'pie',
                    hoverAnimation: false,
                    radius: ['60%', '75%'],
                    center: ['70%', '40%'],
                    startAngle: 225,
                    labelLine: {
                        normal: {
                            show: false
                        }
                    },
                    label: {
                        normal: {
                            position: 'center'
                        }
                    },
                    data: [{
                            value: deprate,
                            itemStyle: {
                                normal: {
                                    color: '#90EE90'
                                }
                            },
                            label: dataStyle,
                        }, {
                            value: 33,
                            itemStyle: placeHolderStyle,
                        },

                    ]
                },
            ]
        };

        // 使用刚指定的配置项和数据显示图表。
        myChart.setOption(option);
        window.addEventListener("resize", function () {
            myChart.resize();
        });
    }


})