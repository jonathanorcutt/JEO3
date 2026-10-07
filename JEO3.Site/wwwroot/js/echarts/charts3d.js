
// ---------------------------------------------------------------------------------
// ECharts
export function renderPanoramic3DChart(containerId, title, xAxisTitle, yAxisTitle, intensityLabel, xAxisData, yAxisData, chartData) {
    var chartContainer = document.getElementById(containerId);
    if (!chartContainer) return;

    var chartInstance = echarts.getInstanceByDom(chartContainer) || echarts.init(chartContainer);

    chartInstance.setOption({
        backgroundColor: "#0a0c10",
        title: {
            text: title,
            textStyle: { color: "#ffffff", fontFamily: "Consolas, monospace", fontSize: 14, fontWeight: "bold" },
            left: "center",
            top: "1%"
        },
        tooltip: {
            trigger: "item",
            backgroundColor: "rgba(10, 12, 16, 0.92)",
            borderColor: "#00ff41",
            borderWidth: 1,
            textStyle: { color: "#ffffff", fontFamily: "Consolas, monospace", fontSize: 11 },
            formatter: function (params) {
                var xName = xAxisData[params.value[0]] || params.value[0];
                var yName = yAxisData[params.value[1]] || params.value[1];
                var zVal = params.value[2].toFixed(3);
                return '<strong>' + params.seriesName + '</strong><br/>' +
                    xAxisTitle + ': ' + xName + '<br/>' +
                    yAxisTitle + ': ' + yName + '<br/>' +
                    intensityLabel + ': ' + zVal;
            }
        },
        visualMap: {
            type: "continuous",
            max: 1.0,
            min: 0.0,
            calculable: true,
            orient: "vertical",
            right: "3%",
            bottom: "15%",
            dimension: 2,
            text: ["High", "Low"],
            textStyle: { color: "#00ff41", fontFamily: "Consolas, monospace" },
            inRange: { color: ["#1e3a8a", "#2563eb", "#22c55e", "#facc15", "#ef4444"] }
        },
        xAxis3D: {
            type: "category",
            data: xAxisData,
            name: xAxisTitle,
            nameTextStyle: { color: "#00ff41", fontFamily: "Consolas, monospace" },
            axisLabel: { show: true, color: "#ffffff", fontFamily: "Consolas, monospace", interval: 0 }
        },
        yAxis3D: {
            type: "category",
            data: yAxisData,
            name: yAxisTitle,
            nameTextStyle: { color: "#00ff41", fontFamily: "Consolas, monospace" },
            axisLabel: { show: true, color: "#ffffff", fontFamily: "Consolas, monospace", interval: 0 }
        },
        zAxis3D: {
            type: "value",
            min: 0.0,
            max: 1.0,
            name: intensityLabel,
            nameTextStyle: { color: "#00ff41", fontFamily: "Consolas, monospace" },
            axisLabel: { show: true, color: "#ffffff", fontFamily: "Consolas, monospace" }
        },
        grid3D: {
            boxWidth: 160,
            boxHeight: 65,
            boxDepth: 180,
            top: "-8%",
            bottom: "20%",
            shading: "lambert",
            viewControl: { autoRotate: false, alpha: 25, beta: 40, distance: 190 },
            light: {
                main: { intensity: 1.3, shadow: true, alpha: 40, beta: 40 },
                ambient: { intensity: 0.4 }
            }
        },
        series: [{
            type: "bar3D",
            name: title,
            data: chartData,
            shading: "lambert",
            label: { show: false },
            itemStyle: { opacity: 0.95 }
        }]
    }, true);
};

// ---------------------------------------------------------------------------------
// ECharts Voxel Cinematic Renderer
export function renderVox3DChart(containerId, title, xAxisTitle, yAxisTitle, intensityLabel, xAxisData, yAxisData, imageSrcPath) {
    var chartContainer = document.getElementById(containerId);
    if (!chartContainer) return;

    var chartInstance = echarts.getInstanceByDom(chartContainer) || echarts.init(chartContainer);

    // 1. Structural look variables
    var config = {
        scale: 0.15,
        roughness: 0,
        metalness: 0.0,
        projection: 'perspective',
        depthOfField: 1,
        lockY: false,
        move: true,
        sameColor: false,
        color: '#777',
        colorContrast: 1.0,
        barNumber: 80,
        barBevel: 0.18,
        barSize: 1.5
    };

    var canvas = document.createElement('canvas');
    var ctx = canvas.getContext('2d');

    // 2. Baseline environmental view (SSAO Post-processing and starting camera tilt)
    var initialOption = {
        tooltip: {},
        backgroundColor: '#0a0c10',
        xAxis3D: { type: 'value' },
        yAxis3D: { type: 'value' },
        zAxis3D: { type: 'value', min: 0, max: 100 },
        animationThreshold: 99999,
        animationDurationUpdate: 10000, // Matches your fallback timeline
        grid3D: {
            show: false,
            viewControl: {
                projection: config.projection,
                alpha: 0,           // Matches your starting flat alignment look
                beta: -360,         // Matches your full rotation launch spin
                distance: 8,        // 🌟 FORCE starting camera location right into the mesh strings!
                // targetCoord:, // 🌟 Shifts camera center right into the physical block cluster center
                panSensitivity: config.move ? 1 : 0,
                rotateSensitivity: config.lockY ? [1, 0] : 1,
                damping: 0.95
            },
            postEffect: {
                enable: true,
                bloom: { intensity: 0.2 },
                screenSpaceAmbientOcclusion: {
                    enable: true,
                    intensity: 2.0,
                    radius: 5,
                    quality: 'high'
                },
                screenSpaceReflection: { enable: true },
                depthOfField: {
                    enable: true,
                    blurRadius: config.depthOfField,
                    fstop: 10,
                    focalDistance: 15
                }
            },
            boxDepth: 100,
            boxHeight: 20,
            environment: 'none',
            light: {
                main: { shadow: true, intensity: 2.0 },
                ambient: { intensity: 0.2 }
            }
        }
    };

    chartInstance.setOption(initialOption, true);

    // 3. Canvas image loading loop
    var img = new Image();
    img.onload = function () {
        var height = (canvas.height = Math.min(config.barNumber, img.height));
        var aspect = img.width / img.height;
        var width = (canvas.width = Math.round(height * aspect));

        ctx.drawImage(img, 0, 0, width, height);
        var imgData = ctx.getImageData(0, 0, width, height);
        var pixelData = imgData.data;

        var data = new Float32Array((pixelData.length / 4) * 3);
        var off = 0;

        for (var i = 0; i < pixelData.length / 4; i++) {
            var r = pixelData[i * 4];
            var g = pixelData[i * 4 + 1];
            var b = pixelData[i * 4 + 2];

            var lum = 0.2125 * r + 0.7154 * g + 0.0721 * b;
            lum = (lum - 125) * config.scale + 50;

            data[off++] = i % width;
            data[off++] = height - Math.floor(i / width);
            data[off++] = lum;
        }

        // 4. Inject the completed series definitions explicitly right here!
        chartInstance.setOption({
            grid3D: {
                boxWidth: (100 / height) * width,
                transitionDuration: 10000,
                transitionEasing: 'cubicOut'
            },
            series: [{
                animation: false,
                type: 'bar3D',
                shading: 'realistic',
                realisticMaterial: {
                    roughness: config.roughness,
                    metalness: config.metalness
                },
                barSize: config.barSize,
                bevelSize: config.barBevel,
                silent: true,
                dimensions: ['x', 'y', 'z'],
                itemStyle: {
                    color: config.sameColor ? config.color : function (params) {
                        var idx = params.dataIndex;
                        var pr = pixelData[idx * 4] / 255;
                        var pg = pixelData[idx * 4 + 1] / 255;
                        var pb = pixelData[idx * 4 + 2] / 255;
                        var plum = 0.2125 * pr + 0.7154 * pg + 0.0721 * pb;

                        pr *= plum * config.colorContrast;
                        pg *= plum * config.colorContrast;
                        pb *= plum * config.colorContrast;
                        return [pr, pg, pb, 1];
                    }
                },
                data: data
            }]
        });

        // 5. Run the smooth transition swoop into the flat front angle view
        setTimeout(function () {
            chartInstance.setOption({
                grid3D: {
                    viewControl: {
                        alpha: 90,                  // Perfectly flat head-on pitch
                        beta: 0,                   // Straight alignment
                        distance: 100,              // Front view framing boundary
                        targetCoord: [null, null, null] // 🌟 Resets target back to automatic layout centering
                    }
                }
            });
        }, 2000);
    };

    img.src = imageSrcPath;
}

// ---------------------------------------------------------------------------------

export async function renderPanoramicBuildings3D(containerId, title, geoJsonPath, tableMetrics) {
    var chartContainer = document.getElementById(containerId);
    if (!chartContainer) return;

    var chartInstance = echarts.getInstanceByDom(chartContainer) || echarts.init(chartContainer);

    try {
        var response = await fetch(geoJsonPath);
        var buildingsGeoJson = await response.json();

        echarts.registerMap('buildings', buildingsGeoJson);

        // Convert your incoming metrics collection into a quick-lookup map array
        // keyed completely on your dynamic TablePath identifiers
        const metricLookup = {};
        if (tableMetrics && Array.isArray(tableMetrics)) {
            tableMetrics.forEach(m => {
                metricLookup[m.tablePath] = m.value;
            });
        }

        var regionsData = buildingsGeoJson.features.map(function (feature) {
            const featureName = feature.properties.name;
            // Extract the live complex metrics value from your database model instance, fallback to 0.1 if missing
            const activeIntensity = metricLookup[featureName] !== undefined ? metricLookup[featureName] : 0.1;

            return {
                name: featureName,
                value: activeIntensity,
                // Scales the physical height dynamically based on your live database telemetry matrix
                height: (feature.properties.height * activeIntensity) / 10
            };
        });

        chartInstance.setOption({
            backgroundColor: "#0a0c10",
            title: {
                text: title,
                textStyle: { color: "#ffffff", fontFamily: "Consolas, monospace", fontSize: 14, fontWeight: "bold" },
                left: "center",
                top: "1%"
            },
            visualMap: {
                show: true,
                min: 0.0,
                max: 1.0,
                orient: "vertical",
                right: "3%",
                bottom: "15%",
                text: ["High", "Low"],
                textStyle: { color: "#00ff41", fontFamily: "Consolas, monospace" },
                inRange: {
                    color: [
                        '#313695', '#4575b4', '#74add1', '#abd9e9', '#e0f3f8',
                        '#ffffbf', '#fee090', '#fdae61', '#f46d43', '#d73027', '#a50026'
                    ]
                }
            },
            series: [{
                type: 'map3D',
                map: 'buildings',
                shading: 'realistic',
                environment: '#0a0c10',
                realisticMaterial: { roughness: 0.6, textureTiling: 20 },
                postEffect: {
                    enable: true,
                    SSAO: { enable: true, radius: 1, intensity: 1.2 }
                },
                viewControl: { alpha: 40, beta: 180, distance: 120, autoRotate: false },
                itemStyle: { opacity: 1.0 },
                data: regionsData
            }]
        }, true);

        setTimeout(function () {
            chartInstance.setOption({ series: [{ silent: true, progressive: true, progressiveThreshold: 500 }] });
        }, 2500);

    } catch (err) {
        console.error("Failed to execute urban WebGL maps render pass: ", err);
    }
}





// 🌟 Append this to the bottom of your charts3d.js module file
/*
 * JEO3 3D Schema Relationship Flight Deck
 *
 * Requires:
 *   ECharts
 *   ECharts-GL
 *
 * ECharts-GL provides:
 *   grid3D
 *   xAxis3D
 *   yAxis3D
 *   zAxis3D
 *   scatter3D
 *   lines3D
 */

const relationSimulations = new Map();

export function renderRelations3DFlights(containerId, title, tablesNodes, foreignKeyLinks) {
    const chartContainer = document.getElementById(containerId);
    if (!chartContainer) { console.warn(`JEO3 3D chart container '${containerId}' was not found.`); return; }
    if (typeof echarts === "undefined") { console.error("JEO3 3D chart: ECharts is not loaded."); return; }

    let chartInstance = echarts.getInstanceByDom(chartContainer);
    if (!chartInstance) { chartInstance = echarts.init(chartContainer); }

    /* 1. Bounds first, so layout x/y can be mapped into lng/lat */
    const validNodes = (tablesNodes || []).filter(node => node && node.id);
    const bounds = calculateBounds(validNodes);
    const spanX = Math.max(1, bounds.maxX - bounds.minX);
    const spanY = Math.max(1, bounds.maxY - bounds.minY);
    const toLngLat = (x, y) => [((x - bounds.minX) / spanX) * 340 - 170, ((y - bounds.minY) / spanY) * 140 - 70];

    /* 2. Node lookup + scatter data, all as [lng, lat, altitude] */
    const nodePositions = Object.create(null);
    const nodeData = [];
    for (const node of validNodes) {
        const [lng, lat] = toLngLat(Number(node.x) || 0, Number(node.y) || 0);
        nodePositions[node.id] = [lng, lat, 0];
        nodeData.push({ name: node.name || node.id, value: [lng, lat, 0] });
    }

    /* 3. Flight paths, split into two groups instead of one series per link */
    const normalPaths = [];
    const hotPaths = [];
    for (const link of foreignKeyLinks || []) {
        if (!link) { continue; }
        const source = nodePositions[link.source];
        const target = nodePositions[link.target];
        if (!source || !target) { continue; }
        const path = createFlightPath(source, target);
        if (path.length < 2) { continue; }
        (link.isHighTraffic ? hotPaths : normalPaths).push(path);
    }

    const linesSeries = (name, data, color, width, opacity) => ({
        name,
        type: "lines3D",
        coordinateSystem: "globe",
        polyline: true,
        blendMode: "lighter",
        data,
        lineStyle: { color, width, opacity },
        distance:5,
        effect: { show: true, trailWidth: 2, trailLength: 0.15, trailOpacity: 1, trailColor: color }
    });

    var texture = '../../img/branding/world.topo.bathy.200401.jpg';
    var bath = '../../img/branding/bathymetry_bw_composite_4k.jpg';
    var star = '../../img/branding/jeo3.png';  

    /* 4. Options */
    const option = { 
        backgroundColor: "#000",
        title: {
            text: title || "Database Relations Flight Simulator",
            left: "center",
            top: "1%",
            textStyle: {
                //color: "#ffffff",
                fontSize: 24,
                fontWeight: "bold",
                color: "#08FC17",
                fontFamily: 'NaziTypewriterRegular, consolas',
                textBorderColor: '#000',
                textBorderType: 'solid',
                textBorderWidth: 2
            }
        },
        globe: {
            heightTexture: bath, 
            // baseColor: '#000', 
            baseTexture: texture, 
            environment: star,
            shading: "realistic",
            displacementScale: 0.1, 
            displacementQuality: "high",
            realisticMaterial: { roughness: 0.2, metalness: 0 },
            postEffect: {
                enable: true,
                depthOfField: {
                    enable: false, focalRange: 150,
                    enable: true,
                    focalDistance: 150
                }
            },
            // temporalSuperSampling: { enable: true },
            light: {
                ambient: { intensity: 0.4 },
                main: { intensity: 0.4, shadow: false },
                ambientCubemap: {
                    texture: "../../assets/data/lake.hdr",
                    exposure: 1,
                    diffuseIntensity: 0.5,
                    specularIntensity: 2
                }
            },
            viewControl: {
                autoRotate: true, autoRotateSpeed: 1, distance: 300, minDistance: 125, maxDistance: 500,
                alpha: 12, beta: 67
                // alpha: 0, beta: 0
            },
            silent: true
        },
        yAxis3D:
        {
            offset: -180
        },
        series: [
            {
                name: "Tables",
                type: "scatter3D",
                distance: 8,
                coordinateSystem: "globe",
                data: nodeData,
                symbol: "circle",
                symbolSize: 10,
                itemStyle: { color: "#0077F2", opacity: 0.95 },
                label: {
                    show: true,
                    formatter: p => p.data.name,
                    textStyle: {
                        color: "#08FC17", fontFamily: "Consolas, monospace", fontSize: 13,
                        fontFamily: 'NaziTypewriterRegular, consolas',
                        textBorderColor: '#000',
                        textBorderType: 'solid',
                        textBorderWidth: 2
                    }
                }
            },
            linesSeries("Normal", normalPaths, "#00ff41", 1, 0.55),
            linesSeries("High Traffic", hotPaths, "#ff00ff", 2.5, 0.9)
        ]
    };

    /* One setOption, one time. No animation loop. */
    chartInstance.setOption(option, true);

    requestAnimationFrame(() => {
        if (!chartInstance.isDisposed()) { chartInstance.resize(); }
    });
}

/* Arc between two [lng, lat, alt] points. Altitude is in globe units (radius is about 100). */
function createFlightPath(source, target) {
    const dx = target[0] - source[0];
    const dy = target[1] - source[1];
    const distance = Math.sqrt((dx * dx) + (dy * dy));

    // const arcHeight = Math.max(4, Math.min(20, distance * 0.08));
    const arcHeight = 1;
    const steps = 32;
    const path = [];

    for (let i = 0; i <= steps; i++) {
        const t = i / steps;
        const arc = 1 * t * (1 - t);
        path.push([source[0] + (dx * t), source[1] + (dy * t), source[2] + ((target[2] - source[2]) * t) + (arcHeight * arc)]);
    }

    return path;
}

/* Bounds of the layout x/y, used to map into lng/lat */
function calculateBounds(nodes) {
    if (!nodes || nodes.length === 0) {
        return { minX: -120, maxX: 120, minY: -120, maxY: 120 };
    }

    let minX = Infinity, maxX = -Infinity, minY = Infinity, maxY = -Infinity;

    for (const node of nodes) {
        const x = Number(node.x) || 0;
        const y = Number(node.y) || 0;
        minX = Math.min(minX, x);
        maxX = Math.max(maxX, x);
        minY = Math.min(minY, y);
        maxY = Math.max(maxY, y);
    }

    const paddingX = Math.max(20, (maxX - minX) * 0.10);
    const paddingY = Math.max(20, (maxY - minY) * 0.10);

    return { minX: minX - paddingX, maxX: maxX + paddingX, minY: minY - paddingY, maxY: maxY + paddingY };
}

export function disposeRelations3D(containerId) {
    const chartContainer = document.getElementById(containerId);
    if (!chartContainer) { return; }

    const chartInstance = echarts.getInstanceByDom(chartContainer);
    if (chartInstance) { chartInstance.dispose(); }
}